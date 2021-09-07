using Sound;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using EaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EaxReverb;
using EfxEaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EfxEaxReverb;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SoundThread : BaseThread
    {
        private EfxEaxReverb _reverbSetting = EaxReverbDefaults.GetDefaultSetting();
        private int _reverbParameterIndex = -1;
        private string[] _reverbParameters =
  {
                "Density",
                "Diffusion",
                "Gain",
                "GainHF",
                "DecayTime",
                "DecayHFRatio",
                "ReflectionsGain",
                "ReflectionsDelay",
                "LateReverbGain",
                "LateReverbDelay",
                "AirAbsorptionGainHF",
                "RoomRolloffFactor",
                "DecayHFLimit",
                "DecayLFRatio",
                "EchoDepth",
                "EchoTime",
                "GainLF",
                "HFReference",
                "LFReference",
                "ModulationDepth",
                "ModulationTime",
                "ReflectionsPan",
                "LateReverbPan"
            };

        public void SayReverbPresetName() => DebugSO.SayDelegate(string.IsNullOrEmpty(ReverbPresetName) ? "nic" : ReverbPresetName);

        public void DecreaseReverbParameter()
        {
            if (DebugSO.TestModeEnabled)
            {
                AdjustReverbParameter(-1);
            }
        }


        private void AdjustReverbParameter(int sign)
        {
            void Adjust(ref float parameter, float defaultValue, float minValue, float maxValue, float delta = .01f) // Local function
            {
                float temp = parameter + sign * delta;

                if (temp >= minValue && temp <= maxValue)
                {
                    parameter = temp;
                }
            }


            switch (_reverbParameterIndex)
            {
                case 0: Adjust(ref _reverbSetting.Density, EaxReverbDefaults.Density, EaxReverbDefaults.MinDensity, EaxReverbDefaults.MaxDensity); break;
                case 1: Adjust(ref _reverbSetting.Diffusion, EaxReverbDefaults.Diffusion, EaxReverbDefaults.MinDiffusion, EaxReverbDefaults.MaxDiffusion); break;
                case 2: Adjust(ref _reverbSetting.Gain, EaxReverbDefaults.Gain, EaxReverbDefaults.MinGain, EaxReverbDefaults.MaxGain); break;
                case 3: Adjust(ref _reverbSetting.GainHF, EaxReverbDefaults.GainHF, EaxReverbDefaults.MinGainHF, EaxReverbDefaults.MaxGainHF); break;
                case 4: Adjust(ref _reverbSetting.DecayTime, EaxReverbDefaults.DecayTime, EaxReverbDefaults.MinDecayTime, EaxReverbDefaults.MaxDecayTime); break;
                case 5: Adjust(ref _reverbSetting.DecayHFRatio, EaxReverbDefaults.DecayHFRatio, EaxReverbDefaults.MinDecayHFRatio, EaxReverbDefaults.MaxDecayHFRatio); break;
                case 6: Adjust(ref _reverbSetting.ReflectionsGain, EaxReverbDefaults.ReflectionsGain, EaxReverbDefaults.MinReflectionsGain, EaxReverbDefaults.MaxReflectionsGain); break;
                case 7: Adjust(ref _reverbSetting.ReflectionsDelay, EaxReverbDefaults.ReflectionsDelay, EaxReverbDefaults.MinReflectionsDelay, EaxReverbDefaults.MaxReflectionsDelay); break;
                case 8: Adjust(ref _reverbSetting.LateReverbGain, EaxReverbDefaults.LateReverbGain, EaxReverbDefaults.MinLateReverbGain, EaxReverbDefaults.MaxLateReverbGain); break;
                case 9: Adjust(ref _reverbSetting.LateReverbDelay, EaxReverbDefaults.LateReverbDelay, EaxReverbDefaults.MinLateReverbDelay, EaxReverbDefaults.MaxLateReverbDelay); break;
                case 10: Adjust(ref _reverbSetting.AirAbsorptionGainHF, EaxReverbDefaults.AirAbsorptionGainHF, EaxReverbDefaults.MinAirAbsorptionGainHF, EaxReverbDefaults.MaxAirAbsorptionGainHF); break;
                case 11: Adjust(ref _reverbSetting.RoomRolloffFactor, EaxReverbDefaults.RoomRolloffFactor, EaxReverbDefaults.MinRoomRolloffFactor, EaxReverbDefaults.MaxRoomRolloffFactor); break;
                case 12: _reverbSetting.DecayHFLimit = sign == 1 ? 1 : 0; break;
                case 13: Adjust(ref _reverbSetting.DecayLFRatio, EaxReverbDefaults.DecayLFRatio, EaxReverbDefaults.MinDecayLFRatio, EaxReverbDefaults.MaxDecayHFRatio); break;
                case 14: Adjust(ref _reverbSetting.EchoDepth, EaxReverbDefaults.EchoDepth, EaxReverbDefaults.MinEchoDepth, EaxReverbDefaults.MaxEchoDepth); break;
                case 15: Adjust(ref _reverbSetting.EchoTime, EaxReverbDefaults.EchoTime, EaxReverbDefaults.MinEchoTime, EaxReverbDefaults.MaxEchoTime); break;
                case 16: Adjust(ref _reverbSetting.GainLF, EaxReverbDefaults.GainLF, EaxReverbDefaults.MinGainLF, EaxReverbDefaults.MaxGainLF); break;
                case 17: Adjust(ref _reverbSetting.HFReference, EaxReverbDefaults.HFReference, EaxReverbDefaults.MinHFReference, EaxReverbDefaults.MaxHFReference); break;
                case 18: Adjust(ref _reverbSetting.LFReference, EaxReverbDefaults.LFReference, EaxReverbDefaults.MinLFReference, EaxReverbDefaults.MaxLFReference); break;
                case 19: Adjust(ref _reverbSetting.ModulationDepth, EaxReverbDefaults.ModulationDepth, EaxReverbDefaults.MinModulationDepth, EaxReverbDefaults.MaxModulationDepth); break;
                case 20: Adjust(ref _reverbSetting.ModulationTime, EaxReverbDefaults.ModulationTime, EaxReverbDefaults.MinModulationTime, EaxReverbDefaults.MaxModulationTime); break;
                case 21:
                case 22:
                    float radian = sign * MathHelper.DegreesToRadians(1);
                    float cos = (float)Math.Cos(radian);
                    float sin = (float)Math.Sin(radian);
                    OpenTK.Vector3 zero = OpenTK.Vector3.Zero;
                    OpenTK.Vector3 forward = new OpenTK.Vector3(0, 0, 1);

                    if (_reverbParameterIndex == 21)
                    {
                        _reverbSetting.ReflectionsPan = _reverbSetting.ReflectionsPan == zero ? forward : new OpenTK.Vector3(cos * _reverbSetting.ReflectionsPan.X - sin * _reverbSetting.ReflectionsPan.Z, 0, sin * _reverbSetting.ReflectionsPan.X + cos * _reverbSetting.ReflectionsPan.Z);
                    }
                    else
                    {
                        _reverbSetting.LateReverbPan = _reverbSetting.LateReverbPan == zero ? forward : new OpenTK.Vector3(cos * _reverbSetting.LateReverbPan.X - sin * _reverbSetting.LateReverbPan.Z, 0, sin * _reverbSetting.LateReverbPan.X + cos * _reverbSetting.LateReverbPan.Z);
                    }

                    break;
            }

            RunCommand(() => _openALSystem.SetEaxReverbProperties(_reverbSetting));
        }

        public void SayReverbParameterValue()
        {
            if (!DebugSO.TestModeEnabled)
            {
                return;
            }

            void Say(float parameter)
                =>                      DebugSO.SayDelegate(parameter.ToString("0.0000"));

            switch (_reverbParameterIndex)
            {
                case 0: Say(_reverbSetting.Density); break;
                case 1: Say(_reverbSetting.Diffusion); break;
                case 2: Say(_reverbSetting.Gain); break;
                case 3: Say(_reverbSetting.GainHF); break;
                case 4: Say(_reverbSetting.DecayTime); break;
                case 5: Say(_reverbSetting.DecayHFRatio); break;
                case 6: Say(_reverbSetting.ReflectionsGain); break;
                case 7: Say(_reverbSetting.ReflectionsDelay); break;
                case 8: Say(_reverbSetting.LateReverbGain); break;
                case 9: Say(_reverbSetting.LateReverbDelay); break;
                case 10: Say(_reverbSetting.AirAbsorptionGainHF); break;
                case 11: Say(_reverbSetting.RoomRolloffFactor); break;
                case 12: DebugSO.SayDelegate(_reverbSetting.DecayHFLimit.ToString()); break;
                case 13: Say(_reverbSetting.DecayLFRatio); break;
                case 14: Say(_reverbSetting.EchoDepth); break;
                case 15: Say(_reverbSetting.EchoTime); break;
                case 16: Say(_reverbSetting.GainLF); break;
                case 17: Say(_reverbSetting.HFReference); break;
                case 18: Say(_reverbSetting.LFReference); break;
                case 19: Say(_reverbSetting.ModulationDepth); break;
                case 20: Say(_reverbSetting.ModulationTime); break;
                case 21: DebugSO.SayDelegate(_reverbSetting.ReflectionsPan.ToString()); break;
                case 22: DebugSO.SayDelegate(_reverbSetting.LateReverbPan.ToString()); break;
            }

        }

        private void SpeakReverbParameter()
          => DebugSO.SayDelegate(_reverbParameters[_reverbParameterIndex]);

        public void IncreaseReverbParameter()
        {
            if (DebugSO.TestModeEnabled)
            {
                AdjustReverbParameter(1);
            }
        }


        public void SetReverbParameterToMinimum()
        {
            if (!DebugSO.TestModeEnabled)
            {
                return;
            }

            DebugSO.SayDelegate("Minimum");

            switch (_reverbParameterIndex)
            {
                case 0: _reverbSetting.Density = EaxReverbDefaults.MinDensity; break;
                case 1: _reverbSetting.Diffusion = EaxReverbDefaults.MinDiffusion; break;
                case 2: _reverbSetting.Gain = EaxReverbDefaults.MinGain; break;
                case 3: _reverbSetting.GainHF = EaxReverbDefaults.MinGainHF; break;
                case 4: _reverbSetting.DecayTime = EaxReverbDefaults.MinDecayTime; break;
                case 5: _reverbSetting.DecayHFRatio = EaxReverbDefaults.MinDecayHFRatio; break;
                case 6: _reverbSetting.ReflectionsGain = EaxReverbDefaults.MinReflectionsGain; break;
                case 7: _reverbSetting.ReflectionsDelay = EaxReverbDefaults.MinReflectionsDelay; break;
                case 8: _reverbSetting.LateReverbGain = EaxReverbDefaults.MinLateReverbGain; break;
                case 9: _reverbSetting.LateReverbDelay = EaxReverbDefaults.MinLateReverbDelay; break;
                case 10: _reverbSetting.AirAbsorptionGainHF = EaxReverbDefaults.MinAirAbsorptionGainHF; break;
                case 11: _reverbSetting.RoomRolloffFactor = EaxReverbDefaults.MinRoomRolloffFactor; break;
                case 12: _reverbSetting.DecayHFLimit = 0; break;
                case 13: _reverbSetting.DecayLFRatio = EaxReverbDefaults.MinDecayLFRatio; break;
                case 14: _reverbSetting.EchoDepth = EaxReverbDefaults.MinEchoDepth; break;
                case 15: _reverbSetting.EchoTime = EaxReverbDefaults.MinEchoTime; break;
                case 16: _reverbSetting.GainLF = EaxReverbDefaults.MinGainLF; break;
                case 17: _reverbSetting.HFReference = EaxReverbDefaults.MinHFReference; break;
                case 18: _reverbSetting.LFReference = EaxReverbDefaults.MinLFReference; break;
                case 19: _reverbSetting.ModulationDepth = EaxReverbDefaults.MinModulationDepth; break;
                case 20: _reverbSetting.ModulationTime = EaxReverbDefaults.MinModulationTime; break;
                case 21: _reverbSetting.ReflectionsPan = OpenTK.Vector3.Zero; break;
                case 22: _reverbSetting.LateReverbPan = OpenTK.Vector3.Zero; break;
            }

            RunCommand(() => _openALSystem.SetEaxReverbProperties(_reverbSetting));
        }

        public void SetReverbParameterToMaximum()
        {
            if (!DebugSO.TestModeEnabled)
                return;

            DebugSO.SayDelegate("Maximum");

            switch (_reverbParameterIndex)
            {
                case 0: _reverbSetting.Density = EaxReverbDefaults.MaxDensity; break;
                case 1: _reverbSetting.Diffusion = EaxReverbDefaults.MaxDiffusion; break;
                case 2: _reverbSetting.Gain = EaxReverbDefaults.MaxGain; break;
                case 3: _reverbSetting.GainHF = EaxReverbDefaults.MaxGainHF; break;
                case 4: _reverbSetting.DecayTime = EaxReverbDefaults.MaxDecayTime; break;
                case 5: _reverbSetting.DecayHFRatio = EaxReverbDefaults.MaxDecayHFRatio; break;
                case 6: _reverbSetting.ReflectionsGain = EaxReverbDefaults.MaxReflectionsGain; break;
                case 7: _reverbSetting.ReflectionsDelay = EaxReverbDefaults.MaxReflectionsDelay; break;
                case 8: _reverbSetting.LateReverbGain = EaxReverbDefaults.MaxLateReverbGain; break;
                case 9: _reverbSetting.LateReverbDelay = EaxReverbDefaults.MaxLateReverbDelay; break;
                case 10: _reverbSetting.AirAbsorptionGainHF = EaxReverbDefaults.MaxAirAbsorptionGainHF; break;
                case 11: _reverbSetting.RoomRolloffFactor = EaxReverbDefaults.MaxRoomRolloffFactor; break;
                case 12: _reverbSetting.DecayHFLimit = 1; break;
                case 13: _reverbSetting.DecayLFRatio = EaxReverbDefaults.MaxDecayLFRatio; break;
                case 14: _reverbSetting.EchoDepth = EaxReverbDefaults.MaxEchoDepth; break;
                case 15: _reverbSetting.EchoTime = EaxReverbDefaults.MaxEchoTime; break;
                case 16: _reverbSetting.GainLF = EaxReverbDefaults.MaxGainLF; break;
                case 17: _reverbSetting.HFReference = EaxReverbDefaults.MaxHFReference; break;
                case 18: _reverbSetting.LFReference = EaxReverbDefaults.MaxLFReference; break;
                case 19: _reverbSetting.ModulationDepth = EaxReverbDefaults.MaxModulationDepth; break;
                case 20: _reverbSetting.ModulationTime = EaxReverbDefaults.MaxModulationTime; break;
                case 21: _reverbSetting.ReflectionsPan = MathHelper.GetUnitVectorFromCompassDegrees(359).AsOpenALVector().AsOpenTKV3(); break;
                case 22: _reverbSetting.LateReverbPan = MathHelper.GetUnitVectorFromCompassDegrees(359).AsOpenALVector().AsOpenTKV3(); break;
            }

            RunCommand(() => _openALSystem.SetEaxReverbProperties(_reverbSetting));
        }

        public void SetReverbParameterToDefault()
        {
            if (!DebugSO.TestModeEnabled)
            {
                return;
            }

            DebugSO.SayDelegate("Výchozí.");

            switch (_reverbParameterIndex)
            {
                case 0: _reverbSetting.Density = EaxReverbDefaults.Density; break;
                case 1: _reverbSetting.Diffusion = EaxReverbDefaults.Diffusion; break;
                case 2: _reverbSetting.Gain = EaxReverbDefaults.Gain; break;
                case 3: _reverbSetting.GainHF = EaxReverbDefaults.GainHF; break;
                case 4: _reverbSetting.DecayTime = EaxReverbDefaults.DecayTime; break;
                case 5: _reverbSetting.DecayHFRatio = EaxReverbDefaults.DecayHFRatio; break;
                case 6: _reverbSetting.ReflectionsGain = EaxReverbDefaults.ReflectionsGain; break;
                case 7: _reverbSetting.ReflectionsDelay = EaxReverbDefaults.ReflectionsDelay; break;
                case 8: _reverbSetting.LateReverbGain = EaxReverbDefaults.LateReverbGain; break;
                case 9: _reverbSetting.LateReverbDelay = EaxReverbDefaults.LateReverbDelay; break;
                case 10: _reverbSetting.AirAbsorptionGainHF = EaxReverbDefaults.AirAbsorptionGainHF; break;
                case 11: _reverbSetting.RoomRolloffFactor = EaxReverbDefaults.RoomRolloffFactor; break;
                case 12: _reverbSetting.DecayHFLimit = EaxReverbDefaults.DecayHFLimit ? 1 : 0; break;
                case 13: _reverbSetting.DecayLFRatio = EaxReverbDefaults.DecayLFRatio; break;
                case 14: _reverbSetting.EchoDepth = EaxReverbDefaults.EchoDepth; break;
                case 15: _reverbSetting.EchoTime = EaxReverbDefaults.EchoTime; break;
                case 16: _reverbSetting.GainLF = EaxReverbDefaults.GainLF; break;
                case 17: _reverbSetting.HFReference = EaxReverbDefaults.HFReference; break;
                case 18: _reverbSetting.LFReference = EaxReverbDefaults.LFReference; break;
                case 19: _reverbSetting.ModulationDepth = EaxReverbDefaults.ModulationDepth; break;
                case 20: _reverbSetting.ModulationTime = EaxReverbDefaults.ModulationTime; break;
                case 21: _reverbSetting.ReflectionsPan = EaxReverbDefaults.ReflectionsPan; break;
                case 22: _reverbSetting.LateReverbPan = EaxReverbDefaults.LateReverbPan; break;
            }

            RunCommand(() => _openALSystem.SetEaxReverbProperties(_reverbSetting));
        }

        public void PreviousReverbParameter()
        {
            if (!DebugSO.TestModeEnabled)
            {
                return;
            }

            if (_reverbParameterIndex > 0)
            {
                _reverbParameterIndex--;
                SpeakReverbParameter();
            }
        }


        public void NextReverbParameter()
        {
            if (!DebugSO.TestModeEnabled)
            {
                return;
            }

            if (_reverbParameterIndex < _reverbParameters.Length - 1)
            {
                _reverbParameterIndex++;
                SpeakReverbParameter();
            }
        }


        private void LoadReverbPresets()
        {
            if (!DebugSO.TestModeEnabled)
            {
                return;
            }

            _reverbPresets = EaxReverbDefaults.GetEaxPresets().ToList();
        }


        public void SwitchToPreviousReverbPreset()
        {
            if (!DebugSO.TestModeEnabled)
                return;

            if (_reverbPresets == null)
                LoadReverbPresets();

            (string Name, EaxReverb Preset) preset = _reverbPresets[_presetInx];
            ReverbPresetName = preset.Name;
            RunCommand(() => _openALSystem.ApplyEaxReverbPreset(preset.Preset, preset.Name));

            if (_presetInx == 0)
                _presetInx = _reverbPresets.Count-1;
            else _presetInx--;

        }


        public void SwitchToNextReverbPreset()
        {
            if (!DebugSO.TestModeEnabled)
                return;

            if (_reverbPresets == null)
                LoadReverbPresets();

            (string Name, EaxReverb Preset) preset = _reverbPresets[_presetInx];
            ReverbPresetName = preset.Name;
            RunCommand(() => _openALSystem.ApplyEaxReverbPreset(preset.Preset, preset.Name));

            if (_presetInx == _reverbPresets.Count -1)
                _presetInx = 0;
            else _presetInx++;

        }

        private List<(string Name,  EaxReverb Preset)> _reverbPresets;
        private int _presetInx;

        public ReadStream GetSoundStream(string name, int variant = 1)
        {
            SoundFileInfo info;
            Assert(_soundFiles.TryGetValue(name.ToLower(), out info), $"Soubor {name} nebyl nalezen.");
            Assert(variant >= 1, $"{nameof(variant)} musí být větší než 0.");

            return ReadStream.FromFileSystem(GetSoundFileInfo(name)[variant].Path.ToString());
        }


        private SoundFileInfo GetSoundFileInfo(string name)
        {
            SoundFileInfo info;
            Assert(_soundFiles.TryGetValue(name.ToLower(), out info), $"Žádná varianta zvuku {name} nebyla nalezena.");
            return info;
        }


        /// <summary>
        /// Maps sound file names to coresponding paths. Allows quickly find a sound file just by name.
        /// </summary>
        public void LoadSounds()
        {
            _soundFiles = new Dictionary<string, SoundFileInfo>();
            IEnumerable<string> files = Directory.EnumerateFiles(DebugSO.SoundAssetsPath, "*.*", SearchOption.AllDirectories).Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".flac") || f.EndsWith(".ogg"));
            HashSet<string> usedNames = new HashSet<string>();
            foreach (string path in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                Assert(!usedNames.Contains(fileName), $"Duplicitní název souboru: {path}.");
                usedNames.Add(fileName);
                string[] nameParts = Regex.Split(fileName, @"\s");
                string shortName = nameParts[0].ToLower();
                int variant;
                Assert(Int32.TryParse(nameParts[1], out variant), $"Chybný název souboru: {path}.");
                variant--;
                Assert(variant >= 0, $"Chybný název souboru: {path}.");

                SoundFileInfo info;
                if (!_soundFiles.TryGetValue(shortName, out info))
                {
                    _soundFiles[shortName] = new SoundFileInfo(shortName, path, variant);
                }
                else if (info.Variants < variant)
                {
                    info.Variants = variant;
                }
            }
        }



        public ReadStream GetRandomSoundStream(string soundName)
         => ReadStream.FromFileSystem(GetSoundFileInfo(soundName).GetRandomVariant());


        private const int _bufferSize = 1920;

        private const int _millisecondsPerTick = 10;


        private readonly ShortBuffer _buffer = new ShortBuffer(_bufferSize);

        private readonly List<ShortBuffer> _buffers;

        private List<Snapshot> _blendingSnapshots = new List<Snapshot>();

        private List<DelayedSound> _delayedSounds = new List<DelayedSound>();

        private Dictionary<Name, float> _groupVolumes = new Dictionary<Name, float>();

        private int _incrementingSoundID;

        private LSFDecoder _lSFDecoder;

        private NAudioDecoder _nAudioDecoder;

        private Action<Exception> _onError;

        private OpenALSystem _openALSystem;
        private OpusFileDecoder _opusFileDecoder;
        private List<Sound> _sounds = new List<Sound>(); // used to generate unique sound IDs.
        private Dictionary<string, SoundFileInfo> _soundFiles; // Maps names and paths of all sound files in data directory.

        /// <summary>
        /// private constructor
        /// </summary>
        /// <param name="onError"></param>
        private SoundThread(Action<Exception> onError)
        {
            _onError = onError;

            _buffers = new List<ShortBuffer>(OpenALSystem.MaxQueuedBuffers);
            for (int i = 0; i < OpenALSystem.MaxQueuedBuffers; i++)
            {
                _buffers.Add(new ShortBuffer(_bufferSize));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static SoundThread CreateAndStartThread(Action<Exception> onError)
        {
            SoundThread soundThread = new SoundThread(onError);
            soundThread.StartThread();
            return soundThread;
        }

        // 1920 shorts = 20ms of PCM stereo samples at 48000Hz.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        public void AddBlendingSnapshot(Snapshot ss)
        { // we could have a race condition if someone changes the Snapshot after they have passed it to us, so we set it as immutable so we will at least get a runtime exception if we ever accidentally write code like that.
            ss.Immutable = true;
            RunCommand(() =>
            { // the collection of blending snapshots is more like a set than a list, hence why we only add it if it is not already present.
                if (!_blendingSnapshots.Contains(ss))
                {
                    _blendingSnapshots.Add(ss);
                }

                RecalculateAllSoundVolumes();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="state"></param>
        /// <param name="currentSample"></param>
        public void GetDynamicInfo(int soundID, out SoundState state, out int currentSample)
        {
            object[] results = RunQuery(() =>
            {
                Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);
                if (sound == null)
                {
                    return new object[] { SoundState.Disposed, -1 };
                }

                IPlayback playback = GetPlayback(sound);
                SoundState ss;
                if (playback.IsPlaying(soundID))
                {
                    ss = SoundState.Playing;
                }
                else if (playback.IsStopped(soundID))
                {
                    ss = SoundState.Disposed; // stopped is the same as disposed for our purposes
                }
                else
                {
                    ss = SoundState.Paused;
                }

                // we subtract the buffered samples because when they are full, playback is actually that far behind the decoder, and the caller wants to know where playback is.
                // we divide by 50 because OpenAL buffers always hold 20ms of PCM data, so sampleRate / 50 is 20ms worth of samples.
                int bufferedSamples = OpenALSystem.MaxQueuedBuffers * sound.SampleRate / 50;

                IDecoder decoder = GetDecoder(sound);
                int tempCurrent;
                decoder.GetDynamicInfo(soundID, out tempCurrent);
                if (tempCurrent != -1)
                {
                    tempCurrent = Math.Max(0, tempCurrent - bufferedSamples);
                }

                return new object[] { ss, tempCurrent };
            });
            state = (SoundState)results[0];
            currentSample = (int)results[1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="sampleRate"></param>
        /// <param name="totalSamples"></param>
        /// <param name="channels"></param>
        public void GetStaticInfo(int soundID, out int sampleRate, out int totalSamples, out int channels)
        {
            object[] results = RunQuery(() =>
            {
                Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);
                if (sound == null)
                {
                    return new object[] { -1, -1, -1 };
                }

                IDecoder decoder = GetDecoder(sound);
                int tempSampleRate, tempTotal, tempChannels;
                decoder.GetStaticInfo(soundID, out tempSampleRate, out tempTotal, out tempChannels);
                return new object[] { tempSampleRate, tempTotal, tempChannels };
            });
            sampleRate = (int)results[0];
            totalSamples = (int)results[1];
            channels = (int)results[2];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="forceMono"></param>
        public void ChangeForceMono(int soundID, bool forceMono) => RunCommand(() =>
                                                                  {
                                                                      Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);
                                                                      if (sound == null)
                                                                      {
                                                                          return;
                                                                      }

                                                                      if (sound.Decoder == Decoder.Opusfile && sound.Playback == Playback.OpenAL)
                                                                      {
                                                                          int channels, sampleRate;
                                                                          _opusFileDecoder.ChangeForceMono(soundID, forceMono, out channels, out sampleRate);
                                                                          // changing to mono on the fly doesn't seem to work well with OpenAL, perhaps the source can't be changed to use a different number of channels after it has started.
                                                                          _openALSystem.ReconfigureSound(soundID, channels, sampleRate);
                                                                      }
                                                                  });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="looping"></param>
        public void ChangeLooping(int soundID, bool looping) => RunCommand(() =>
                                                              {
                                                                  Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);
                                                                  if (sound == null)
                                                                  {
                                                                      return;
                                                                  }

                                                                  IDecoder decoder = GetDecoder(sound);
                                                                  decoder.ChangeLooping(soundID, looping);
                                                              });

        public Vector3 ListenerVelocity
        {
            get => _listenerVelocity;
            set
            {
                _listenerVelocity = value;
                RunCommand(() => _openALSystem.SetListenerVelocity(value));
            }
        }

        public Vector3 ListenerPosition
        {
            get => _listenerPosition;
            set
            {
                _listenerPosition = value;
                RunCommand(() => _openALSystem.SetListenerPosition(value));
            }
        }


        public Vector3 ListenerOrientationFacing
        {
            get => _listenerOrientationFacing;
            set
            {
                _listenerOrientationFacing = value;
                RunCommand(() => _openALSystem.SetListenerOrientation(value, _listenerOrientationUp));
            }
        }

        public Vector3 ListenerOrientationUp
        {
            get => _listenerOrientationUp;
            set
            {
                _listenerOrientationUp = value;
                RunCommand(() => _openALSystem.SetListenerOrientation(_listenerOrientationFacing, value));
            }
        }
        private Vector3 _listenerVelocity;
        private Vector3 _listenerPosition;
        private Vector3 _listenerOrientationFacing;
        private Vector3 _listenerOrientationUp;
        public string ReverbPresetName;

        public void SetSourcePosition(int soundID, Vector3 soundPosition)
            => RunCommand(() => _openALSystem.SetPosition(soundID, soundPosition));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listenerPosition"></param>
        /// <param name="listenerFacing"></param>
        /// <param name="listenerUp"></param>
        /// <param name="soundIDs"></param>
        /// <param name="soundPositions"></param>
        public void SetPositions(Vector3 listenerPosition, Vector3 listenerFacing, Vector3 listenerUp, params (int id, Vector3 position)[] sourceParameters)
        {
            ListenerPosition = ListenerPosition;
            ListenerOrientationFacing = listenerFacing;
            ListenerOrientationUp = listenerUp;

            // The parameter values are copied to new variables in order to prevent racing condition.
            RunCommand(() =>
                {
                    for (int i = 0; i < _sounds.Count; i++)
                    {
                        int id = sourceParameters[i].id;
                        Vector3 position = sourceParameters[i].position;
                        Sound sound = _sounds.FirstOrDefault(s => s.ID == id);

                        if (sound != null)
                        {
                            _openALSystem.SetPosition(id, position);
                        }
                    }
                });
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="role"></param>
        /// <param name="volumeAdjustment"></param>
        /// <param name="seekSample"></param>
        /// <returns></returns>
        public int Play(ReadStream stream, Name role = null, float volumeAdjustment = 1f, int seekSample = 0)
        => Play(stream, role, false, PositionType.None, Vector3.Zero, false, volumeAdjustment, seekSample: seekSample);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundName"></param>
        /// <param name="role"></param>
        /// <param name="looping"></param>
        /// <param name="pt"></param>
        /// <param name="position"></param>
        /// <param name="forceMono"></param>
        /// <param name="volumeAdjustment"></param>
        /// <param name="panning"></param>
        /// <param name="pitch"></param>
        /// <param name="seekSample"></param>
        /// <param name="pb"></param>
        /// <returns>the soundID</returns>
        public int Play(string soundName, Name role, bool looping, PositionType pt, Vector3 position, bool forceMono = false, float volumeAdjustment = 1f, float? panning = null, float pitch = 1f, int seekSample = 0, Playback pb = Playback.OpenAL)
        => Play(GetSoundStream(soundName), role, looping, pt, position, forceMono, volumeAdjustment, panning, pitch, seekSample, pb);

        public int Play(string soundName, Name role, bool looping, PositionType pt, Vector2 position, bool forceMono = false, float volumeAdjustment = 1f, float? panning = null, float pitch = 1f, int seekSample = 0, Playback pb = Playback.OpenAL)
=> Play(soundName, role, looping, pt, position.AsOpenALVector(), forceMono, volumeAdjustment, panning, pitch, seekSample, pb);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundName"></param>
        /// <param name="role"></param>
        /// <param name="volumeAdjustment"></param>
        /// <param name="seekSample"></param>
        /// <returns></returns>
        public int Play(string soundName, Name role = null, float volumeAdjustment = 1f, int seekSample = 0)
        => Play(GetSoundStream(soundName), role, false, PositionType.None, Vector3.Zero, false, volumeAdjustment, seekSample: seekSample);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="role"></param>
        /// <param name="looping"></param>
        /// <param name="pt"></param>
        /// <param name="position"></param>
        /// <param name="forceMono"></param>
        /// <param name="volumeAdjustment"></param>
        /// <param name="panning"></param>
        /// <param name="pitch"></param>
        /// <param name="seekSample"></param>
        /// <param name="pb"></param>
        /// <returns>the soundID</returns>
        public int Play(ReadStream stream, Name role, bool looping, PositionType pt, Vector3 position, bool forceMono = false, float volumeAdjustment = 1f, float? panning = null, float pitch = 1f, int seekSample = 0, Playback pb = Playback.OpenAL)
        {

            if (pt != PositionType.None && panning.HasValue)
            {
                throw new InvalidOperationException("PositionType must be set to None if panning has a value");
            }

            if (stream == null)
            {
                return -1; // missing the sound file, but don't crash. The owner should report this error on their own.
            }

            int soundID = Interlocked.Increment(ref _incrementingSoundID);

            Action callback = () =>
                {
                    // separating init and play is helpful if we want to start a sound playing at a location other than the beginning, because we can call init then seek instead.
                    Sound sound = InnerInitSound(stream, role, looping, pb, soundID, pt, position, forceMono, volumeAdjustment, panning, pitch);
                    if (sound == null)
                    {
                        return;
                    }

                    IDecoder decoder = GetDecoder(sound);
                    if (seekSample != 0)
                    {
                        decoder.SeekToSample(soundID, seekSample);
                    }

                    InnerPlay(sound);
                };

            if (stream.Stream is IAssetStream && !((IAssetStream)stream.Stream).IsPrimed())
            { // if it is not primed
                DelayedSound ds = new DelayedSound();
                ds.SoundID = soundID;
                ds.ReadStream = stream;
                ds.Callback = callback;
                _delayedSounds.Add(ds);
            }
            else // it was either a normal stream, or an AssetStream that is primed
            {
                RunCommand(callback);
            }

            return soundID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ss"></param>
        public void RemoveBlendingSnapshot(Snapshot ss) => RunCommand(() =>
                                                         {
                                                             _blendingSnapshots.Remove(ss);
                                                             RecalculateAllSoundVolumes();
                                                         });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="sampleOffset"></param>
        public void Seek(int soundID, int sampleOffset) => RunCommand(() =>
                                                         {
                                                             Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);
                                                             if (sound == null)
                                                             {
                                                                 return;
                                                             }

                                                             IDecoder decoder = GetDecoder(sound);
                                                             decoder.SeekToSample(soundID, sampleOffset);
                                                             InnerPlay(sound);
                                                         });

        // snapshot and group volume region

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="volume"></param>
        public void SetGroupVolume(Name groupName, float volume) => RunCommand(() =>
                                                                  {
                                                                      _groupVolumes[groupName] = volume;
                                                                      RecalculateAllSoundVolumes();
                                                                  });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="desiredPauseState"></param>
        public void SetPauseState(int soundID, bool desiredPauseState) => RunCommand(() =>
                                                                        {
                                                                            Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);
                                                                            if (sound == null)
                                                                            {
                                                                                return;
                                                                            }

                                                                            IPlayback playback = GetPlayback(sound);
                                                                            if (desiredPauseState)
                                                                            {
                                                                                playback.Pause(soundID);
                                                                            }
                                                                            else
                                                                            {
                                                                                playback.Unpause(soundID);
                                                                            }
                                                                        });

        /// <summary>
        /// 
        /// </summary>
        public void SpeakHRTF()
        => RunCommand(_openALSystem.SpeakHRTF);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        public void Stop(int soundID) => RunCommand(() =>
                                       {
                                           // check for a sound that hadn't started playing yet.
                                           DelayedSound ds = _delayedSounds.RemoveFirstOrDefault(p => p.SoundID == soundID);
                                           if (ds != null)
                                           {
                                               ds.ReadStream.Stream.Dispose();
                                           }
                                           // otherwise we check for a normal playing sound.
                                           Sound sound = _sounds.RemoveFirstOrDefault(p => p.ID == soundID);
                                           if (sound == null)
                                           {
                                               return;
                                           }

                                           IPlayback playback = GetPlayback(sound);
                                           playback.Stop(sound.ID);
                                           DisposeSound(sound);
                                       });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        public void TogglePause(int soundID) => RunCommand(() =>
                                              {
                                                  Sound sound = _sounds.FirstOrDefault(p => p.ID == soundID);
                                                  if (sound == null)
                                                  {
                                                      return;
                                                  }

                                                  IPlayback playback = GetPlayback(sound);
                                                  if (playback.IsPlaying(soundID))
                                                  {
                                                      playback.Pause(soundID);
                                                  }
                                                  else
                                                  {
                                                      playback.Unpause(soundID);
                                                  }
                                              });

        /// <summary>
        /// 
        /// </summary>
        protected override void OnTick()
        {
            // logic for updating streaming buffers and detecting finished sounds that need to be disposed goes here.
            // we dispose sounds when they stop, but not when they are paused.
            // if a caller pauses a sound, they are responsible for unpausing it so it can play to completion, or eventually calling stop on it, which will dispose it.

            // detect delayed sounds that are now primed
            for (int i = _delayedSounds.Count - 1; i >= 0; i--)
            {
                DelayedSound ds = _delayedSounds[i];
                IAssetStream stream = (IAssetStream)ds.ReadStream.Stream;
                if (stream.IsPrimed())
                {
                    _delayedSounds.RemoveAt(i);
                    ds.Callback();
                }
            } // end foreach delayed sound in reverse

            // refill streaming buffers
            foreach (Sound sound in _sounds)
            {
                IPlayback playback = GetPlayback(sound);
                // perhaps this should be while instead of if
                while (playback.IsReadyForBuffer(sound.ID))
                {
                    IDecoder decoder = GetDecoder(sound);
                    _buffer.Filled = decoder.FillBuffer(sound.ID, _buffer.Data);
                    //if (length != 1920)
                    //    Say(length);
                    if (sound.Panning.HasValue)
                    {
                        SoundHM.Pan(_buffer, sound.Panning.Value);
                    }

                    playback.QueueBuffer(sound.ID, _buffer);
                }
            } // end foreach sound

            // detect and remove sounds that finished playing
            for (int i = _sounds.Count - 1; i >= 0; i--)
            {
                Sound sound = _sounds[i];
                IPlayback playback = GetPlayback(sound);
                if (playback.IsStopped(sound.ID))
                {
                    DisposeSound(sound);
                    _sounds.RemoveAt(i);
                }
            } // end foreach sound
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        /// <returns></returns>
        private float CalculateVolume(Sound sound)
        {
            float product = sound.IndividualVolume;
            foreach (Name gn in sound.GroupNames)
            {
                float modifier;
                // mGroupVolumes contains the volume adjustments the user can change in settings
                if (_groupVolumes.TryGetValue(gn, out modifier))
                {
                    product *= modifier;
                }
                // apply blending snapshots, such as the mod snapshot file, or the built in overlay and grid snapshots.
                foreach (Snapshot blend in _blendingSnapshots)
                {
                    if (blend.GroupVolumes.TryGetValue(gn, out modifier))
                    {
                        product *= modifier;
                    }
                } // end foreach blending snapshot
            } // end foreach group name
            return product;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        private void DisposeSound(Sound sound)
        {
            IDecoder decoder = GetDecoder(sound);
            decoder.DisposeStream(sound.ID);
            IPlayback playback = GetPlayback(sound);
            playback.DisposeSound(sound.ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        /// <returns></returns>
        private IDecoder GetDecoder(Sound sound)
        {
            if (sound.Decoder == Decoder.Opusfile)
            {
                return _opusFileDecoder;
            }
            else if (sound.Decoder == Decoder.Libsndfile)
            {
                return _lSFDecoder;
            }
            else if (sound.Decoder == Decoder.NAudio)
            {
                return _nAudioDecoder;
            }
            else
            {
                throw Exception("Unrecognized decoder system: {0}", sound.Decoder);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        /// <returns></returns>
        private IPlayback GetPlayback(Sound sound)
        {
            if (sound.Playback == Playback.OpenAL)
            {
                return _openALSystem;
            }
            else
            {
                throw Exception("Unrecognized playback system: {0}", sound.Playback);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="role"></param>
        /// <param name="looping"></param>
        /// <param name="pb"></param>
        /// <param name="soundID"></param>
        /// <param name="pt"></param>
        /// <param name="position"></param>
        /// <param name="forceMono"></param>
        /// <param name="volumeAdjustment"></param>
        /// <param name="panning"></param>
        /// <param name="pitch"></param>
        /// <returns></returns>
        private Sound InnerInitSound(ReadStream rs, Name role, bool looping, Playback pb, int soundID, PositionType pt, Vector3 position, bool forceMono, float volumeAdjustment, float? panning, float pitch)
        {

            Sound sound = new Sound();
            _sounds.Add(sound);
            sound.ID = soundID;
            sound.Playback = pb;
            sound.IndividualVolume = volumeAdjustment;
            sound.Path = rs.Path;
            sound.Panning = panning;

            sound.GroupNames.Add(Names.NonSpatialized);
            if (role != null)
            {
                sound.GroupNames.Add(role);
            }

            string extension = Path.GetExtension(rs.Path).ToLower();
            if (extension == ".opus")
            {
                sound.Decoder = Decoder.Opusfile;
            }
            else if (extension == ".mp3")
            {
                sound.Decoder = Decoder.NAudio;
            }
            else
            {
                sound.Decoder = Decoder.Libsndfile;
            }

            Stream stream = rs.Stream;
            IDecoder decoder = GetDecoder(sound);
            int channels, sampleRate;
            if (panning.HasValue)
            {
                forceMono = true; // if a sound is going to be panned, we force it to mono so we can later split it to stereo based on the panning.
            }

            decoder.InitStream(soundID, stream, looping, forceMono, out channels, out sampleRate);
            if (panning.HasValue)
            {
                channels = 2; // if a sound is going to be panned, we will always split it to stereo for playback
            }

            sound.SampleRate = sampleRate;
            IPlayback playback = GetPlayback(sound);
            playback.InitSound(soundID, channels, sampleRate, pt, position, pitch);
            playback.SetVolume(soundID, CalculateVolume(sound));
            return sound;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound"></param>
        private void InnerPlay(Sound sound)
        {
            IDecoder decoder = GetDecoder(sound);
            foreach (ShortBuffer buffer in _buffers)
            {
                buffer.Filled = decoder.FillBuffer(sound.ID, buffer.Data);
                // interestingly, the first buffer of every Opusfile decoded sound is always 1296 shorts.
                //if (buffer.Filled != 1920)
                //    Say(buffer.Filled);
                if (sound.Panning.HasValue)
                {
                    SoundHM.Pan(buffer, sound.Panning.Value);
                }
            }
            IPlayback playback = GetPlayback(sound);
            playback.Play(sound.ID, _buffers);
        }

        /// <summary>
        /// 
        /// </summary>
        private void RecalculateAllSoundVolumes()
        {
            foreach (Sound sound in _sounds)
            {
                IPlayback playback = GetPlayback(sound);
                playback.SetVolume(sound.ID, CalculateVolume(sound));
            } // end foreach sound
        }


        public void ApplyEaxReverbPreset(string name, float gain)
        {
            EaxReverb preset = _reverbPresets.First(p => p.Name.ToLower() == name.ToLower()).Preset;
                      RunCommand(() => _openALSystem.ApplyEaxReverbPreset(preset, null, gain));
        }


        /// <summary>
        /// 
        /// </summary>
        private void StartThread()
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    // init logic
                    _openALSystem = OpenALSystem.CreateAndBindToThisThread(true);
                    _lSFDecoder = new LSFDecoder();
                    _opusFileDecoder = new OpusFileDecoder();
                    _nAudioDecoder = new NAudioDecoder();
                    ProcessMessages(_millisecondsPerTick);
                }
                catch (Exception ex)
                { _onError(ex); }
                finally
                { // cleanup logic
                  // the one downside to performing cleanup here is that we don't become aware of exceptions thrown during cleanup.
                    _openALSystem.Dispose();
                    _opusFileDecoder.Dispose();
                    _lSFDecoder.Dispose();
                    _nAudioDecoder.Dispose();
                }
            });

            t.IsBackground = true; // this ensures it closes when the main thread closes, safe for threads that read from files, but not for those that write to files. Though Environment.Exit takes care of this when an exception is caught, and for normal shutdown scenarios we should be calling dispose, so I'm leaving this commented.
            t.Start();
            RunCommand(() => _openALSystem.EnableReverb());
            LoadReverbPresets();
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class DelayedSound
        {
            // a sound that was delayed because it was not yet primed.
            public Action Callback;
            public ReadStream ReadStream;
            public int SoundID;
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class Sound
        {
            public Decoder Decoder;

            public List<Name> GroupNames = new List<Name>()
            {
                Names.Master, // for controlling a single master volume on all sounds.
				Names.Window, // adjusted when the window gains and loses focus.
			};

            public int ID; // a unique identifier.
            public float IndividualVolume = 1f;
            public float? Panning;
            public FilePath Path;
            public Playback Playback = Playback.OpenAL;

            // helpful when debugging.
            // set to null if we are not panning, which is different than setting it to 0, since we must force mono to later split into stereo if we are panning.
            public int SampleRate;
        } // cls
    } // cls
}