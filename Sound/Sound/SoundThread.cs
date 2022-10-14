using OpenTK;

using Sound;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using EaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EaxReverb;

namespace Luky
{
    /// <summary>
    /// A sound player running in a separate thread
    /// </summary>
    public sealed partial class SoundThread : BaseThread
    {
        /// <summary>             /// <summary>
        /// Volume used with sound attenuation.
        /// </summary>
        public float GetOverWallVolume(float defaultVolume)
            => defaultVolume * .4f;

        /// <summary>
        /// Volume used with sound attenuation.
        /// </summary>
        public float GetOverDoorVolume(float defaultVolume)
            => defaultVolume * .5f;

        /// <summary>
        /// Volume used with sound attenuation.
        /// </summary>
        public float GetOverObjectVolume(float defaultVolume)
            => defaultVolume * .9f;

        /// <summary>
        /// Lowpass setting for simulation of sounds obstructed by an object.
        /// </summary>
        public (float gain, float gainHF) OverWallLowpass = (.9f, .03f);

        /// <summary>
        /// Lowpass setting for simulation of sounds over wall.
        /// </summary>
        public (float gain, float gainHF) OverObjectLowpass = (.7f, .8f);

        /// <summary>
        /// Lowpass setting for simulation of sounds obstructed by a door.
        /// </summary>
        public (float gain, float gainHF) OverDoorLowpass = (.3f, .5f);

        /// <summary>
        /// Removes the currently applied filter from the specified source.
        /// </summary>
        /// <param name="id">Handle of the source</param>
        public void DisableLowpass(int id)
            => RunCommand(() => _OpenAL.DisableFilters(id));

        /// <summary>
        /// Deactivates the lowpass filter on the specified source.
        /// </summary>
        /// <param name="sourceID">Handle of the source to be affected</param>
        public void CancelLowpass(int sourceID)
            => RunCommand(() => _OpenAL.CancelLowpass(sourceID));

        /// <summary>
        /// Applies lowpass filter setting on a sound.
        /// </summary>
        /// <param name="soundID">Handle of the sound</param>
        /// <param name="gain">Lowpass gain</param>
        /// <param name="gainHF">Lowpass high frequency gain</param>
        public void ApplyLowpass(int soundID, (float gain, float gainHF) parameters)
            => RunCommand(() => _OpenAL.ApplyLowpassFilter(soundID, parameters));

        /// <summary>
        /// Stores planned lowpass filter settings.
        /// </summary>
        private Dictionary<int, (float gain, float gainHF)> _lowpassSettings = new Dictionary<int, (float gain, float gainHF)>();

        /// <summary>
        /// Default general volume
        /// </summary>
        public readonly float DefaultMasterVolume = .5f;

        /// Indicates if the sounds are muted.
        /// </summary>
        public bool Muted;

        /// <summary>
        /// Stores last value of master volume before muting.
        /// </summary>
        private float _masterVolumeBackup;

        /// <summary>
        /// Temporarely fades all sounds out.
        /// </summary>
        public void Mute()
        {
            Muted = true;
            _masterVolumeBackup = _groupVolumes["master"];
            FadeMasterOut(.0001f, 0);
        }

        /// <summary>
        /// Unmutes all sounds to last volume.
        /// </summary>
        public void Unmute()
        {
            if (!Muted)
                return;
            Muted = false;

            FadeMasterIn(.0003f, _masterVolumeBackup);
            _masterVolumeBackup = 0;
        }

        /// <summary>
        /// Name of current EAX reverb preset
        /// </summary>
        public string ReverbPresetName;

        /// <summary>
        /// Size of one sound buffer
        /// </summary>
        private const int _bufferSize = 1920;

        /// <summary>
        /// Sleep time for thread of the player.
        /// </summary>
        private const int _millisecondsPerTick = 10;

        /// <summary>
        /// Path to sound asset folder
        /// </summary>
        private static string _soundPath;

        /// <summary>
        /// Delegate using for text output
        /// </summary>
        private static Action<string> Say;

        /// <summary>
        /// A buffer used for reading from sound files
        /// </summary>
        private readonly ShortBuffer _buffer = new ShortBuffer(_bufferSize);

        /// <summary>
        /// Stores data of currently played sounds.
        /// </summary>
        private readonly List<ShortBuffer> _buffers;

        /// <summary>
        /// Volumes of individual sound groups
        /// </summary>
        private readonly Dictionary<Name, float> _groupVolumes = new Dictionary<Name, float>();

        /// <summary>
        /// An error handler delegate
        /// </summary>
        private readonly Action<Exception, string> _onError;

        /// <summary>
        /// stores peaces of sounds to be played.
        /// </summary>
        private List<DelayedSound> _delayedSounds = new List<DelayedSound>();

        /// <summary>
        /// Used when assigning sound idS
        /// </summary>
        private int _incrementingSoundID;

        /// <summary>
        /// The at vector defining the listener orientation
        /// </summary>
        private Vector3 _listenerOrientationFacing;

        /// <summary>
        /// The up vector defining the listener orientation
        /// </summary>
        private Vector3 _listenerOrientationUp;

        /// <summary>
        /// Current listener position
        /// </summary>
        private Vector3 _listenerPosition;

        /// <summary>
        /// Current listener velocity
        /// </summary>
        private Vector3 _listenerVelocity;

        /// <summary>
        /// Reference to instance of the libsndfile wrapper
        /// </summary>
        private LSFDecoder _lSFDecoder;

        /// <summary>
        /// Reference to instance of the NAudio player decoder used with MP3 files
        /// </summary>
        private NAudioDecoder _nAudioDecoder;

        /// <summary>
        /// Reference to the OpenAL player
        /// </summary>
        private OpenALSystem _OpenAL;

        /// <summary>
        /// Reference to the opus sound files decoder used with opus files
        /// </summary>
        private OpusFileDecoder _opusFileDecoder;

        /// <summary>
        /// Stores all parameters for the EAX reverb effect
        /// </summary>

        /// <summary>
        /// All available EAX reverb presets
        /// </summary>
        private List<(string Name, EaxReverb Preset)> _reverbPresets;

        /// <summary>
        /// All sound files used in the game
        /// </summary>
        private Dictionary<string, SoundFileInfo> _soundFiles;

        /// <summary>
        /// currently played sounds.
        /// </summary>
        private List<Sound> _sounds = new List<Sound>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="onError">An error handler delegate</param>
        /// <param name="say">Text output delegate</param>
        private SoundThread(string soundPath, Action<Exception, string> onError, Action<string> say)
        {
            _soundPath = soundPath;
            Say = say;
            _onError = onError;

            _buffers = new List<ShortBuffer>(OpenALSystem.MaxQueuedBuffers);
            for (int i = 0; i < OpenALSystem.MaxQueuedBuffers; i++)
                _buffers.Add(new ShortBuffer(_bufferSize));
        }

        /// <summary>
        /// The at vector for listener orientation
        /// </summary>
        public Vector3 ListenerOrientationFacing
        {
            get => _listenerOrientationFacing;
            set
            {
                _listenerOrientationFacing = value;
                RunCommand(() => _OpenAL.SetListenerOrientation(value, _listenerOrientationUp));
            }
        }

        /// <summary>
        /// The up vector for listener orientation
        /// </summary>
        public Vector3 ListenerOrientationUp
        {
            get => _listenerOrientationUp;
            set
            {
                _listenerOrientationUp = value;
                RunCommand(() => _OpenAL.SetListenerOrientation(_listenerOrientationFacing, value));
            }
        }

        /// <summary>
        /// current listener position
        /// </summary>
        public Vector3 ListenerPosition
        {
            get => _listenerPosition;
            set
            {
                _listenerPosition = value;
                RunCommand(() => _OpenAL.SetListenerPosition(value));
            }
        }

        /// <summary>
        /// Current listener velocity
        /// </summary>
        public Vector3 ListenerVelocity
        {
            get => _listenerVelocity;
            set
            {
                _listenerVelocity = value;
                RunCommand(() => _OpenAL.SetListenerVelocity(value));
            }
        }

        /// <summary> Creates and runs the player. </summary> <param name="soundPath" <param
        /// name="onError">An error handler delegate</param> <param name="say">A delegate for text
        /// output</param> <returns>New instance of the player</returns>
        public static SoundThread CreateAndStartThread(string soundPath, Action<Exception, string> onError, Action<string> say)
        {
            SoundThread soundThread = new SoundThread(soundPath, onError, say);
            soundThread.StartThread();
            return soundThread;
        }

        /// <summary>
        /// This method will pan a mono sound into interleaved stereo, outputting into the same
        /// buffer it was given
        /// </summary>
        /// <param name="sBuffer">The sound buffer</param>
        /// <param name="panning">Panning value in range from 0 to 1</param>
        public static void Pan(ShortBuffer sBuffer, float panning)
        {
            if (panning < -1 || panning > 1)
                throw new ArgumentException("panning must be between -1 and 1");

            int inLength = sBuffer.Filled;
            short[] buffer = sBuffer.Data;
            int outLength = inLength * 2;

            if (buffer.Length < outLength)
                throw new Exception("Pan requires a buffer that is at least twice the length of the input data");

            float leftMultiplier = (1 - panning) / 2;
            float rightMultiplier = (1 + panning) / 2;

            // Fill the buffer in reverse, so the same buffer can be used for input and output.
            for (int monoIndex = inLength - 1; monoIndex >= 0; monoIndex--)
            {
                // Read the mono bytes in reverse order. Output the interleaved stereo bytes in
                // reverse order.
                int stereoIndex = monoIndex * 2;
                short monoVal = buffer[monoIndex];
                buffer[stereoIndex] = (short)Math.Round(monoVal * leftMultiplier);
                buffer[stereoIndex + 1] = (short)Math.Round(monoVal * rightMultiplier);
            }

            sBuffer.Filled *= 2;
        }

        public void SetEaxReverbReflectionsGain(float value)
            => RunCommand(() => _OpenAL.SetEaxReverbReflectionsGain(value));

        /// <summary>
        /// Changes value of the EAX reverb reflections parameter.
        /// </summary>
        /// <param name="pan">The pan vector</param>
        public void SetEaxReverbReflections(Vector3 pan)
            => RunCommand(() => _OpenAL.SetEaxReverbReflectionsPan(pan));

        /// <summary>
        /// Enables an EAX reverb preset.
        /// </summary>
        /// <param name="name">Name of the preset</param>
        /// <param name="gain">Reverb gain parameter</param>
        public void ApplyEaxReverbPreset(string name, float gain)
        {
            EaxReverb preset = _reverbPresets.First(p => p.Name.ToLower() == name.ToLower()).Preset;
            RunCommand(() => _OpenAL.ApplyEaxReverbPreset(preset, null, gain));
        }

        /// <summary>
        /// Optains dynamic information about the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="state">Reference to a variable to store sound state</param>
        /// <param name="currentSample">Reference to a variable to store current sample</param>
        public void GetDynamicInfo(int soundID, out SoundState state, out int currentSample)
        {
            object[] results = RunQuery(() =>
            {
                Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);

                if (sound == null)
                    return new object[] { SoundState.Disposed, -1 };

                IPlayback playback = GetPlayback(sound);
                SoundState ss;

                if (playback.IsPlaying(soundID))
                    ss = SoundState.Playing;
                else if (playback.IsStopped(soundID))
                    ss = SoundState.Disposed; // stopped is the same as disposed for our purposes
                else
                    ss = SoundState.Paused;

                // I subtract the buffered samples because when they are full, playback is actually
                // that far behind the decoder, and the caller wants to know where playback is. I
                // divide by 50 because OpenAL buffers always hold 20ms of PCM data, so sampleRate /
                // 50 is 20ms worth of samples.
                int bufferedSamples = OpenALSystem.MaxQueuedBuffers * sound.SampleRate / 50;

                IDecoder decoder = GetDecoder(sound);
                decoder.GetDynamicInfo(soundID, out int tempCurrent);

                if (tempCurrent != -1)
                    tempCurrent = Math.Max(0, tempCurrent - bufferedSamples);

                return new object[] { ss, tempCurrent };
            });

            state = (SoundState)results[0];
            currentSample = (int)results[1];
        }

        /// <summary>
        /// Sets volume of an individual source.
        /// </summary>
        /// <param name="id">Handle of the source to be affected</param>
        /// <param name="volume">Target volume of the specified source</param>
        public void SetSourceVolume(int id, float volume)
        {
            RunCommand(() =>
            {
                GetPlayback(GetSound(id))
.SetVolume(id, volume);
            }
            );
        }

        /// <summary>
        /// Returns stream of a random variant of the specified sound file.
        /// </summary>
        /// <param name="soundName">The sound</param>
        /// <returns>The stream</returns>
        public ReadStream GetRandomSoundStream(string soundName)
         => ReadStream.FromFileSystem(GetSoundFileInfo(soundName).GetRandomVariant());

        /// <summary>
        /// Returns stream of a sound file variant.
        /// </summary>
        /// <param name="name">The sound</param>
        /// <param name="variant">Number of the variant</param>
        /// <returns>The stream</returns>
        public ReadStream GetSoundStream(string name, int variant = 1)
        {
            Assert(_soundFiles.ContainsKey(name.ToLower()), $"Soubor {name} nebyl nalezen.");
            Assert(variant >= 1, $"{nameof(variant)} musí být větší než 0.");

            return ReadStream.FromFileSystem(GetSoundFileInfo(name)[variant].Path.ToString());
        }

        /// <summary>
        /// Optains static info about the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="sampleRate">Reference to a variable to store sample rate</param>
        /// <param name="totalSamples">Reference to a variable to store count of samples</param>
        /// <param name="channels">Reference to a variable to store number of channels</param>
        public void GetStaticInfo(int soundID, out int sampleRate, out int totalSamples, out int channels)
        {
            object[] results = RunQuery(() =>
            {
                Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);

                if (sound == null)
                    return new object[] { -1, -1, -1 };

                IDecoder decoder = GetDecoder(sound);
                decoder.GetStaticInfo(soundID, out int tempSampleRate, out int tempTotal, out int tempChannels);
                return new object[] { tempSampleRate, tempTotal, tempChannels };
            });

            sampleRate = (int)results[0];
            totalSamples = (int)results[1];
            channels = (int)results[2];
        }

        /// <summary>
        /// Sets the parameter that decides if the sound should be converted to mono.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="forceMono">Specifies if the sound should be converted to mono.</param>
        public void ChangeForceMono(int soundID, bool forceMono)
        {
            RunCommand(() =>
               {
                   Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);

                   if (sound == null || sound.Decoder != Decoder.Opusfile || sound.Playback != Playback.OpenAL)
                       return;

                   _opusFileDecoder.ChangeForceMono(soundID, forceMono, out int channels, out int sampleRate);
                   _OpenAL.ReconfigureSound(soundID, channels, sampleRate);
               });
        }
        /// <summary>
        /// Enables or disables looping.
        /// </summary>
        /// <param name="soundID">ID of the sound to be modified</param>
        /// <param name="looping">Specifies if looping is enabled or disabled</param>
        public void ChangeLooping(int soundID, bool looping) => RunCommand(() =>
        {
            Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);

            if (sound == null)
                return;

            IDecoder decoder = GetDecoder(sound);
            decoder.ChangeLooping(soundID, looping);
        });

        /// <summary>
        /// Maps sound file names to coresponding paths.
        /// </summary>
        public void LoadSounds()
        {
            _soundFiles = new Dictionary<string, SoundFileInfo>();
            IEnumerable<string> files = Directory.EnumerateFiles(_soundPath, "*.*", SearchOption.AllDirectories).Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".flac") || f.EndsWith(".ogg"));
            HashSet<string> usedNames = new HashSet<string>();

            foreach (string path in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                Assert(!usedNames.Contains(fileName), $"Duplicate file name: {path}.");
                usedNames.Add(fileName);
                string[] nameParts = Regex.Split(fileName, @"\s");
                string shortName = nameParts[0].ToLower();
                Assert(Int32.TryParse(nameParts[1], out int variant), $"Missing file name: {path}.");
                Assert(variant >= 0, $"Missing file name: {path}.");

                if (!_soundFiles.TryGetValue(shortName, out SoundFileInfo info))
                {
                    _soundFiles[shortName] = new SoundFileInfo(shortName, path, variant);
                }
                else if (info.Variants < variant)
                    info.Variants = variant;
            }
        }

        /// <summary>
        /// Plays a sound stream.
        /// </summary>
        /// <param name="stream">The sound stream to be played</param>
        /// <param name="role">Name of the sound group to which this sound should be assigned</param>
        /// <param name="volumeAdjustment">Individual volume of the sound</param>
        /// <param name="seekSample">The sample from which to start playback</param>
        /// <returns>Handle of the sound</returns>
        public int Play(ReadStream stream, Name role = null, float volumeAdjustment = 1f, int seekSample = 0)
        => Play(stream, role, false, PositionType.None, Vector3.Zero, false, volumeAdjustment, seekSample: seekSample);

        /// <summary>
        /// Plays a sound file.
        /// </summary>
        /// <param name="soundName">Path to the sound file to be played</param>
        /// <param name="role">Name of the sound group to which this sound should be assigned</param>
        /// <param name="looping">Specifies if the sound should be played in a loop</param>
        /// <param name="positionType">Specifies how the sound position is calculated.</param>
        /// <param name="position">
        /// Three-dimensional vector that determines the position in the virtual sound space
        /// </param>
        /// <param name="forceMono">Specifies whether to play a stereo sound in mono.</param>
        /// <param name="volumeAdjustment">Individual volume of the sound</param>
        /// <param name="panning">Specifies if the sound should be panned</param>
        /// <param name="pitch">Specifies speed of playback</param>
        /// <param name="seekSample">The sample from which to start playback</param>
        /// <param name="pb">Specifies which player to play the audio with.</param>
        /// <returns>Handle of the sound</returns>
        public int Play(string soundName, Name role, bool looping, PositionType positionType, Vector3 position, bool forceMono = false, float volumeAdjustment = 1f, float? panning = null, float pitch = 1f, int seekSample = 0, Playback pb = Playback.OpenAL)
        => Play(GetSoundStream(soundName), role, looping, positionType, position, forceMono, volumeAdjustment, panning, pitch, seekSample, pb);

        /// <summary>
        /// Plays a sound file.
        /// </summary>
        /// <param name="soundName">Path to the sound file</param>
        /// <param name="role">Name of the sound group to which this sound should be assigned</param>
        /// <param name="looping">Specifies if the sound should be played in a loop</param>
        /// <param name="pt">Specifies how the sound position is calculated.</param>
        /// <param name="position">
        /// Two-dimensional vector that determines the position in the virtual sound space
        /// </param>
        /// <param name="forceMono">Specifies whether to play a stereo sound in mono.</param>
        /// <param name="volumeAdjustment">Individual volume of the sound</param>
        /// <param name="panning">Specifies if the sound should be panned</param>
        /// <param name="pitch">Specifies speed of playback</param>
        /// <param name="seekSample">The sample from which to start playback</param>
        /// <param name="pb">Specifies which player to play the audio with.</param>
        /// <returns>Handle of the sound</returns>
        public int Play(string soundName, Name role, bool looping, PositionType pt, Vector2 position, bool forceMono = false, float volumeAdjustment = 1f, float? panning = null, float pitch = 1f, int seekSample = 0, Playback pb = Playback.OpenAL)
        => Play(soundName, role, looping, pt, position.AsOpenALVector(), forceMono, volumeAdjustment, panning, pitch, seekSample, pb);

        /// <summary>
        /// Plays a sound file.
        /// </summary>
        /// <param name="soundName">Path to the sound file</param>
        /// <param name="role">Name of the sound group to which this sound should be assigned</param>
        /// <param name="volumeAdjustment">Individual volume of the sound</param>
        /// <param name="seekSample">The sample from which to start playback</param>
        /// <returns>Handle of the sound</returns>
        public int Play(string soundName, Name role = null, float volumeAdjustment = 1f, int seekSample = 0)
        => Play(GetSoundStream(soundName), role, false, PositionType.None, Vector3.Zero, false, volumeAdjustment, seekSample: seekSample);

        /// <summary>
        /// Plays a sound stream.
        /// </summary>
        /// <param name="stream">The stream to be played</param>
        /// <param name="role">Name of the sound group to which this sound should be assigned</param>
        /// <param name="looping">Specifies if the sound should be played in a loop</param>
        /// <param name="pt">Specifies how the sound position is calculated.</param>
        /// <param name="position">
        /// Three-dimensional vector that determines the position in the virtual sound space
        /// </param>
        /// <param name="forceMono">Specifies whether to play a stereo sound in mono.</param>
        /// <param name="volumeAdjustment">Individual volume of the sound</param>
        /// <param name="panning">Specifies if the sound should be panned</param>
        /// <param name="pitch">Specifies speed of playback</param>
        /// <param name="seekSample">The sample from which to start playback</param>
        /// <param name="pb">Specifies which player to play the audio with.</param>
        /// <returns>Handle of the sound</returns>
        public int Play(ReadStream stream, Name role, bool looping, PositionType pt, Vector3 position, bool forceMono = false, float volumeAdjustment = 1f, float? panning = null, float pitch = 1f, int seekSample = 0, Playback pb = Playback.OpenAL)
        {
            if (pt != PositionType.None && panning.HasValue)
                throw new InvalidOperationException("PositionType must be set to None if panning has a value");

            if (stream == null)
                return -1; // missing the sound file, but don't crash. The owner should report this error on their own.

            int soundID = Interlocked.Increment(ref _incrementingSoundID);

            void callback()
            {
                Sound sound = InnerInitSound(stream, role, looping, pb, soundID, pt, position, forceMono, volumeAdjustment, panning, pitch);
                if (sound == null)
                    throw new InvalidOperationException(nameof(Play));

                IDecoder decoder = GetDecoder(sound);

                if (seekSample != 0)
                    decoder.SeekToSample(soundID, seekSample);

                InnerPlay(sound);
            }

            if (stream.Stream is IAssetStream s && !s.IsPrimed())
            {
                DelayedSound ds = new DelayedSound
                {
                    SoundID = soundID,
                    ReadStream = stream,
                    Callback = callback
                };
                _delayedSounds.Add(ds);
            }
            else // It was either a normal stream or an AssetStream that is primed.
                RunCommand(callback);

            return soundID;
        }

        /// <summary>
        /// Jumps to the specified position of a sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="sampleOffset">The position</param>
        public void Seek(int soundID, int sampleOffset) => RunCommand(() =>
                                                         {
                                                             Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);
                                                             if (sound == null)
                                                                 return;

                                                             IDecoder decoder = GetDecoder(sound);
                                                             decoder.SeekToSample(soundID, sampleOffset);
                                                             InnerPlay(sound);
                                                         });

        /// <summary>
        /// Changes volume of a sound group
        /// </summary>
        /// <param name="groupName">Name of the sound group</param>
        /// <param name="volume">The target volume</param>
        public void SetGroupVolume(Name groupName, float volume) => RunCommand(() =>
                                                                  {
                                                                      _groupVolumes[groupName] = volume;
                                                                      RecalculateAllSoundVolumes();
                                                                  });

        /// <summary>
        /// Pauses or resumes the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="paused">Specifies if the sound should be paused</param>
        public void SetSourceState(int soundID, bool paused)
            => RunCommand(() =>
                                                                        {
                                                                            Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);
                                                                            if (sound == null)
                                                                                return;

                                                                            IPlayback playback = GetPlayback(sound);
                                                                            if (paused)
                                                                                playback.Pause(soundID);
                                                                            else
                                                                                playback.Unpause(soundID);
                                                                        });

        /// <summary>
        /// Sets position of a sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="position">
        /// A two-dimensional vecotr defining the position in virtual sound space
        /// </param>
        public void SetSourcePosition(int soundID, Vector3 position, PositionType type = PositionType.Absolute)
            => RunCommand(() => _OpenAL.SetPosition(soundID, position, type));

        /// <summary>
        /// stops the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        public void Stop(int soundID) => RunCommand(() =>
                                       {
                                           // Check for a sound that hadn't started playing yet.
                                           DelayedSound ds = _delayedSounds.RemoveFirstOrDefault(p => p.SoundID == soundID);

                                           if (ds != null)
                                               ds.ReadStream.Stream.Dispose();

                                           // Otherwise check for a normal playing sound.
                                           Sound sound = _sounds.RemoveFirstOrDefault(p => p.ID == soundID);

                                           if (sound == null)
                                               return;

                                           // Stop fading
                                           if (_fadings.ContainsKey(soundID))
                                               _fadings.Remove(soundID);

                                           // Stop the sound.
                                           IPlayback playback = GetPlayback(sound);

                                           if (playback.IsPlaying(sound.ID))
                                               playback.Stop(sound.ID);
                                           DisposeSound(sound);
                                       });

        /// <summary>
        /// stops all sounds.
        /// </summary>
        public void StopAll() => RunCommand(() =>
        {
            // stop fading effects
            _fadings = new Dictionary<int, FadingRecord>();

            // Check for sounds that hadn't started playing yet.
            foreach (DelayedSound ds in _delayedSounds)
            {
                if (ds != null)
                    ds.ReadStream.Stream.Dispose();
            }

            _delayedSounds = new List<DelayedSound>();

            // Otherwise check for normal playing sounds.
            foreach (Sound sound in _sounds)
            {
                if (sound != null)
                {
                    IPlayback playback = GetPlayback(sound);
                    playback.Stop(sound.ID);
                    DisposeSound(sound);
                }
            }
            _sounds = new List<Sound>();
        });

        /// <summary>
        /// Updates streaming buffers and detects finished sounds that need to be disposed.
        /// </summary>
        /// <remarks>
        /// The method disposes sounds when they stop, but not when they are paused. If a caller
        /// pauses a sound, it's responsible for unpausing or stopping it.
        /// </remarks>
        protected override void OnTick()
        {
            ManageDelayedSounds(); // Detect delayed sounds that are now primed.
            RefillBuffers(); // Refill streaming buffers.
            ReleaseSounds(); // Detect and remove sounds that finished playing.
            MasterFading();
            SourceFading();
        }

        /// <summary>
        /// Currently planned fading for master volume
        /// </summary>
        private FadingRecord _masterFading;

        /// <summary>
        /// Applies a fade in effect on the master volume.
        /// </summary>
        /// <param name="volumeDelta">Specifies how much the master volume changes in one step.</param>
        /// <param name="targetVolume">The final master volume</param>
        public void FadeMasterIn(float volumeDelta, float targetVolume)
            => FadeMaster(FadingType.In, volumeDelta, targetVolume);

        /// <summary>
        /// Fades all currently playing sounds to zero.
        /// </summary>
        /// <param name="volumeDelta">Specifies how much the volume is changed in one step.</param>
        public void FadeAndStopAll(float volumeDelta)
        {
            foreach (Sound s in _sounds)
                _fadings[s.ID] = new FadingRecord(FadingType.Out, volumeDelta, 0);
        }

        /// <summary>
        /// Applies a fade out effect on the master volume.
        /// </summary>
        /// <param name="volumeDelta">Specifies how much the master volume changes in one step.</param>
        /// <param name="targetVolume">The final master volume</param>
        public void FadeMasterOut(float volumeDelta, float targetVolume)
            => FadeMaster(FadingType.Out, volumeDelta, targetVolume);

        /// <summary>
        /// Applies a fading effect on master volume.
        /// </summary>
        /// <param name="type">Type of the fading effect</param>
        /// <param name="volumeDelta">Specifies how much the volume is changed in one step.</param>
        /// <param name="targetVolume">Specifies the final master volume</param>
        public void FadeMaster(FadingType type, float volumeDelta, float targetVolume)
            => _masterFading = new FadingRecord(type, volumeDelta, targetVolume);

        /// <summary>
        /// Performs master fading
        /// </summary>
        private void MasterFading()
        {
            if (_masterFading == null)
                return;

            if (
                (_masterFading.Type == FadingType.In && _groupVolumes["master"] >= _masterFading.TargetVolume)
                || (_masterFading.Type == FadingType.Out && _groupVolumes["master"] <= _masterFading.TargetVolume + _masterFading.VolumeDelta)
                )
            {
                SetGroupVolume("master", _masterFading.TargetVolume);
                _masterFading = null;
            }
            else
                SetGroupVolume("master", _groupVolumes["master"] + (_masterFading.Type == FadingType.In ? _masterFading.VolumeDelta : -_masterFading.VolumeDelta));
        }

        /// <summary>
        /// Performs sound fading.
        /// </summary>
        private void SourceFading()
        {
            Sound sound;
            FadingRecord fading;
            IPlayback playback;

            void Apply(int handle, float volume)
            {
                sound.IndividualVolume = volume;
                RunCommand(() => playback.SetVolume(handle, volume));
            }

            // Change pending fadings to active if the corresponding sounds are playing.
            foreach (int id in _fadings.Keys.ToArray<int>())
            {
                // Ensure the sound hasn't expired.
                fading = _fadings[id];
                fading.Ticks++;
                if (fading.Ticks++ > 11000)
                {
                    _inactiveFadings.Add(id);
                    continue;
                }

                // Check if the faded sound is loaded.
                if ((sound = GetSound(id)) == null)
                    continue;

                // Remove stopped sounds
                playback = GetPlayback(sound);
                if (!playback.IsPlaying(id))
                {
                    _inactiveFadings.Add(id);
                    continue;
                }

                // Fade the sound.
                if (
                    (fading.Type == FadingType.In && sound.IndividualVolume >= fading.TargetVolume)
                    || (fading.Type == FadingType.Out && sound.IndividualVolume <= fading.TargetVolume + fading.VolumeDelta)
                    )
                    _inactiveFadings.Add(id);
                else Apply(id, fading.Type == FadingType.In ? sound.IndividualVolume + fading.VolumeDelta : sound.IndividualVolume - fading.VolumeDelta);
            }

            // Delete unused fadings
            foreach (int id in _inactiveFadings)
            {
                FadingRecord f = _fadings[id];
                if (f.Type == FadingType.Out && f.Stop)
                    Stop(id);

                _fadings.Remove(id);
            }
            _inactiveFadings = new List<int>();
        }

        /// <summary>
        /// Stores handles of sounds with finsihed or cancelled fadings.
        /// </summary>
        private List<int> _inactiveFadings = new List<int>();

        /// <summary>
        /// Performs volume fading on the specified sound source.
        /// </summary>
        /// <param name="soundID">Handle of the sound to be faded</param>
        /// <param name="type">Specifies if the soudn should be faded in or out.</param>
        /// <param name="volumeDelta">Specifies how much the volume changes in one step.</param>
        /// <param name="targetVolume">Specifies the final volume of the sound source</param>
        /// <param name="stop">Specifies if the sound should be stopped after it was muted.</param>
        public void FadeSource(int soundID, FadingType type, float volumeDelta, float targetVolume = 0, bool stop = true)
            => RunCommand(() => _fadings[soundID] = new FadingRecord(type, volumeDelta, targetVolume, stop));

        /// <summary>
        /// Stores records about sound fading.
        /// </summary>
        private Dictionary<int, FadingRecord> _fadings = new Dictionary<int, FadingRecord>();

        /// <summary>
        /// Releases delayed sounds that are primed.
        /// </summary>
        private void ManageDelayedSounds()
        {
            for (int i = _delayedSounds.Count - 1; i >= 0; i--)
            {
                DelayedSound ds = _delayedSounds[i];
                IAssetStream stream = (IAssetStream)ds.ReadStream.Stream;

                if (stream.IsPrimed())
                {
                    _delayedSounds.RemoveAt(i);
                    ds.Callback();
                }
            }

        }

        /// <summary>
        /// Refills sound buffers if needed.
        /// </summary>
        private void RefillBuffers()
        {
            foreach (Sound sound in _sounds)
            {
                IPlayback playback = GetPlayback(sound);

                while (playback.IsReadyForBuffer(sound.ID))
                {
                    IDecoder decoder = GetDecoder(sound);
                    _buffer.Filled = decoder.FillBuffer(sound.ID, _buffer.Data);

                    if (sound.Panning.HasValue)
                        Pan(_buffer, sound.Panning.Value);

                    playback.QueueBuffer(sound.ID, _buffer);
                }
            }

        }

        /// <summary>
        /// Removes sounds that finished playback.
        /// </summary>
        private void ReleaseSounds()
        {
            for (int i = _sounds.Count - 1; i >= 0; i--)
            {
                Sound sound = _sounds[i];
                IPlayback playback = GetPlayback(sound);

                if (playback.IsStopped(sound.ID))
                {
                    DisposeSound(sound);
                    _sounds.RemoveAt(i);
                }
            }

        }

        /// <summary>
        /// Calculates individual volume for a sound.
        /// </summary>
        /// <param name="sound">The sound whose volume is to be calculated</param>
        /// <returns>The individual volume of the sound</returns>
        private float CalculateVolume(Sound sound)
        {
            float product = sound.IndividualVolume;

            foreach (Name groupName in sound.GroupNames)
            {
                if (_groupVolumes.TryGetValue(groupName, out float modifier))
                    product *= modifier;
            }
            return product;
        }

        /// <summary>
        /// Disposes the specified sound.
        /// </summary>
        /// <param name="sound">The sound to be disposed</param>
        private void DisposeSound(Sound sound)
        {
            IDecoder decoder = GetDecoder(sound);
            decoder.DisposeStream(sound.ID);
            IPlayback playback = GetPlayback(sound);
            playback.DisposeSound(sound.ID);
        }

        /// <summary>
        /// Associates a decoder to the specified sound.
        /// </summary>
        /// <param name="sound">The sound</param>
        /// <returns>The decoder</returns>
        private IDecoder GetDecoder(Sound sound)
        {
            switch (sound.Decoder)
            {
                case Decoder.Opusfile:
                    return _opusFileDecoder;

                case Decoder.Libsndfile:
                    return _lSFDecoder;

                case Decoder.NAudio:
                    return _nAudioDecoder;

                default:
                    throw new InvalidOperationException($"Unrecognized decoder system:" + sound.Decoder.ToString());
            }
        }

        /// <summary>
        /// Associates a player to the specified sound.
        /// </summary>
        /// <param name="sound">The sound</param>
        /// <returns>The player</returns>
        private IPlayback GetPlayback(Sound sound)
        {
            if (sound.Playback == Playback.OpenAL)
                return _OpenAL;
            throw new InvalidOperationException($"Unrecognized playback system: {sound.Playback}");
        }

        /// <summary>
        /// Returns information about a sound file.
        /// </summary>
        /// <param name="name">The sound</param>
        /// <returns>A struct describing the sound file</returns>
        private SoundFileInfo GetSoundFileInfo(string name)
        {
            if (!_soundFiles.TryGetValue(name.ToLower(), out SoundFileInfo info))
                throw new InvalidOperationException($"No variant of {name} sound was found.");

            return info;
        }

        /// <summary>
        /// Prepares a sound.
        /// </summary>
        /// <param name="rs">A sound stream</param>
        /// <param name="role">Name of the sound group to which this sound should be assigned</param>
        /// <param name="looping">Specifies if the sound should be played in a loop</param>
        /// <param name="pb">Specifies which player to play the audio with.</param>
        /// <param name="soundID">An ID to be associated with the sound</param>
        /// <param name="pt">Specifies how the sound position is calculated.</param>
        /// ///
        /// <param name="position">
        /// Three-dimensional vector that determines the position in the virtual sound space
        /// </param>
        /// <param name="forceMono">Specifies whether to play a stereo sound in mono.</param>
        /// <param name="volumeAdjustment">Individual volume of the sound</param>
        /// <param name="panning">Specifies if the sound should be panned</param>
        /// <param name="pitch">Specifies speed of playback</param>
        /// <returns>The sound object</returns>
        private Sound InnerInitSound(ReadStream rs, Name role, bool looping, Playback pb, int soundID, PositionType pt, Vector3 position, bool forceMono, float volumeAdjustment, float? panning, float pitch)
        {
            Sound sound = new Sound();
            _sounds.Add(sound);
            sound.ID = soundID;
            sound.Playback = pb;
            sound.IndividualVolume = volumeAdjustment;
            sound.Path = rs.Path;
            sound.Panning = panning;

            sound.GroupNames.Add(new Name("nonspatialized"));

            if (role != null)
                sound.GroupNames.Add(role);

            string extension = Path.GetExtension(rs.Path).ToLower();

            if (extension == ".opus")
                sound.Decoder = Decoder.Opusfile;
            else if (extension == ".mp3")
                sound.Decoder = Decoder.NAudio;
            else
                sound.Decoder = Decoder.Libsndfile;

            Stream stream = rs.Stream;
            IDecoder decoder = GetDecoder(sound);

            if (panning.HasValue)
                forceMono = true;
            //if (rs.Path.Contains("czech"))
            //{
            //    Say("hraje " +sound.ID.ToString());
            //    System.Diagnostics.Debugger.Break();
            //}
            decoder.InitStream(soundID, stream, looping, forceMono, out int channels, out int sampleRate);

            if (panning.HasValue)
                channels = 2; // If a sound is going to be panned, split it to stereo for playback.

            sound.SampleRate = sampleRate;
            IPlayback playback = GetPlayback(sound);
            playback.InitSound(soundID, channels, sampleRate, pt, position, pitch);
            playback.SetVolume(soundID, CalculateVolume(sound));
            return sound;
        }

        /// <summary>
        /// starts playback of a sound.
        /// </summary>
        /// <param name="sound">The sound to be played</param>
        private void InnerPlay(Sound sound)
        {
            IDecoder decoder = GetDecoder(sound);

            foreach (ShortBuffer buffer in _buffers)
            {
                buffer.Filled = decoder.FillBuffer(sound.ID, buffer.Data); // The first buffer of every Opusfile decoded sound is always 1296 shorts.

                if (sound.Panning.HasValue)
                    Pan(buffer, sound.Panning.Value);
            }
            IPlayback playback = GetPlayback(sound);
            playback.Play(sound.ID, _buffers);
        }

        /// <summary>
        /// Preapres EAX reverb presets.
        /// </summary>
        private void LoadReverbPresets()
            => _reverbPresets = EaxReverbDefaults.GetEaxPresets().ToList();

        /// <summary>
        /// Returns a sound record assigned to the specified handle.
        /// </summary>
        /// <param name="id">Handle of the requested sound</param>
        /// <returns>a Sound struct</returns>
        private Sound GetSound(int id)
            => _sounds.FirstOrDefault(s => s.ID == id);

        /// <summary>
        /// Recalculates all volumes after an individual volume was changed.
        /// </summary>
        private void RecalculateAllSoundVolumes()
        {
            foreach (Sound sound in _sounds)
            {
                IPlayback playback = GetPlayback(sound);
                playback.SetVolume(sound.ID, CalculateVolume(sound));
            }
        }

        /// <summary>
        /// starts the player in a separate thread.
        /// </summary>
        private void StartThread()
        {
            Thread t = new Thread(() =>
            {
                //try
                //{
                // init logic
                _OpenAL = OpenALSystem.CreateAndBindToThisThread(Say);
                _lSFDecoder = new LSFDecoder();
                _opusFileDecoder = new OpusFileDecoder();
                _nAudioDecoder = new NAudioDecoder();

                // Start thread loop
                ProcessMessages(_millisecondsPerTick);
                //}
                //catch (Exception ex)
                //{ 
                //_onError(ex); 
                //}
                //finally
                //{
                // cleanup logic The one downside to performing cleanup here is that exceptions
                // thrown during cleanup are discarded.
                //_openALSystem.Dispose();
                //_opusFileDecoder.Dispose();
                //_lSFDecoder.Dispose();
                //_nAudioDecoder.Dispose();
                //}
            });

            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true; // this ensures it closes when the main thread closes, safe for threads that read from files, but not for those that write to files. Though Environment.Exit takes care of this when an exception is caught, and for normal shutdown scenarios we should be calling dispose, so I'm leaving this commented.
            t.Start();
            RunCommand(() => _OpenAL.EnableReverb());
            LoadReverbPresets();
        }
    }
}