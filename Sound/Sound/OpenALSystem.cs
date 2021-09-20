using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using static Sound.EaxReverbDefaults;

using EaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EaxReverb;
using EfxEaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EfxEaxReverb;

namespace Luky
{
    /// <summary>
    /// An OpenAL player
    /// </summary>
    internal sealed class OpenALSystem : DebugSO, IPlayback
    {
        /// <summary>
        /// Maximum allowed number of buffers filled in the same time
        /// </summary>
        public const int MaxQueuedBuffers = 15;

        /// <summary>
        /// The up vector of listener orientation
        /// </summary>
        private static readonly Vector3 _up = new Vector3(0, 0, 1);

        /// <summary>
        /// slot used for effect binding
        /// </summary>
        private static int _effectSlot;

        /// <summary>
        /// Audio context used for communication with the audio device
        /// </summary>
        private AudioContext _alContext;

        /// <summary>
        /// Instance of the effect extension
        /// </summary>
        private EffectsExtension _effectExtension;

        /// <summary>
        /// Handle of an reverb effect 
        /// </summary>
        private int _effectHandle;

        // Maps soundIDs to their associated info.
        private Dictionary<int, Info> _table = new Dictionary<int, Info>();

        /// <summary>
        /// A delegate used for text output
        /// </summary>
        private Action<string> Say;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">An audio context</param>
        /// <param name="say">Delegate for text output</param>
        private OpenALSystem(AudioContext context, Action<string> say)
        {
            Say = say;
            _alContext = context;
        }

        /// <summary>
        /// Initializes OpenAL and starts thread. OpenAL is binded to current thread. Should be used
        /// instead of constructor.
        /// </summary>
        /// <returns>instance of OpenALSystem</returns>
        public static OpenALSystem CreateAndBindToThisThread(Action<string> say)
            => new OpenALSystem(new AudioContext(null, 0, 0, false, true, AudioContext.MaxAuxiliarySends.UseDriverDefault), say);

        /// <summary>
        /// Sets reverb parameters to the specified preset
        /// </summary>
        /// <param name="preset">The reverb preset to be applied</param>
        /// <param name="name">Name of the preset to be spoken (optional)</param>
        /// <param name="gain">Reverb gain parameter</param>
        public void ApplyEaxReverbPreset(EaxReverb preset, string name = null, float gain = 0)
        {
            EaxReverb eaxReverb = preset;
            EfxEaxReverb efxReverb;
            EffectsExtension.GetEaxFromEfxEax(ref eaxReverb, out efxReverb);
            ValidateReverbPreset(ref efxReverb);
            efxReverb.Gain = gain;
            SetEaxReverbProperties(efxReverb, name);
        }

        /// <summary>
        /// Binds the specified sound source to the auxiliary effect slot.
        /// </summary>
        /// <param name="soundId">ID of the source</param>
        public void BindSourceToAuxiliarysend(int soundId)
        {
            if (_effectSlot != 0)
                AL.Source(_table[soundId].SourceID, ALSource3i.EfxAuxiliarySendFilter, _effectSlot, 0, 0);
        }

        /// <summary>
        /// Disposes the player.
        /// </summary>
        public void Dispose()
        => _alContext.Dispose();

        /// <summary>
/// Disposes the specified sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        public void DisposeSound(int soundID)
        {
            Info info = _table[soundID];
            AlDeleteSource(info.SourceID);
            ALAnnounceError("AlDeleteSource");

            foreach (int bid in info.QueuedBufferIDs)
            {
                AL.DeleteBuffer(bid);
                ALAnnounceError("AL.DeleteBuffer");
            } // end foreach queued buffer ID

            _table.Remove(soundID);
        }

        /// <summary>
        /// Turns the reverb effect on.
        /// </summary>
        public void EnableReverb()
        {
            _effectExtension = new EffectsExtension();
            ALAnnounceError("Creating instance of EffectsExtension.");

            _effectHandle = _effectExtension.GenEffect();
            ALAnnounceError("Geneffect.");

            _effectExtension.BindEffect(_effectHandle, EfxEffectType.EaxReverb);
            ALAnnounceError("efx.BindEffect");

            _effectSlot = _effectExtension.GenAuxiliaryEffectSlot();
            ALAnnounceError("Creating effect slot.");
        }

        /// <summary>
        /// Initializes a sound stream.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="stream">The stream to be read</param>
        /// <param name="looping">Specifies if looping is enabled or disabled</param>
        /// <param name="forceMono">Specifies if the sound should be converted to mono</param>
        /// <param name="channels">Reference to a variable to store number of channels of the sound</param>
        /// <param name="sampleRate">Reference to a variable to store sample rate of the sound</param>
        public void InitSound(int soundID, int channels, int sampleRate, PositionType pt, Vector3 position, float frequencyMultiplier)
        {
            Info info = new Info();
            info.Channels = channels;
            info.SampleRate = sampleRate;
            info.SourceID = AL.GenSource();
            ALAnnounceError("AL.GenSource");

            AL.Source((uint)info.SourceID, ALSource3i.EfxAuxiliarySendFilter, _effectSlot, 0, 0);

            if (pt == PositionType.None)
            {
                // Set the source to have relative coordinates so it will always play in the
                // center. this is apparently required even when using the direct channels extension.
                AL.Source(info.SourceID, ALSourceb.SourceRelative, true);
                ALAnnounceError("setting source relative to true");

                AL.Source(info.SourceID, ALSourceb.EfxDirectFilterGainHighFrequencyAuto, true); // using the OpenAL Soft direct channels extension to get rid of sound coloring that occurs when HRTF is enabled.
                ALAnnounceError("setting direct channels to true");
            }

            else if (pt == PositionType.Relative)
            {
                AL.Source(info.SourceID, ALSourceb.SourceRelative, true);
                ALAnnounceError("setting source relative to true");

                ALSetPosition(info.SourceID, position.AsOpenTKV3());
            }

            else if (pt == PositionType.Absolute)
            {
                ALSetPosition(info.SourceID, position.AsOpenTKV3());
                AL.DistanceModel(ALDistanceModel.LinearDistanceClamped);
                AL.Source(info.SourceID, ALSourcef.ReferenceDistance, 1);
                ALAnnounceError("set reference distance");

                AL.Source(info.SourceID, ALSourcef.RolloffFactor, 1);
                ALAnnounceError("set rolloff factor");

                AL.Source(info.SourceID, ALSourcef.MaxDistance, 50);
                ALAnnounceError("set max distancefactor");
            }

            if (frequencyMultiplier != 1f)
            {
                AL.Source(info.SourceID, ALSourcef.Pitch, frequencyMultiplier);
                ALAnnounceError("setting pitch to {0}", frequencyMultiplier);
            }

            _table[soundID] = info;
        }

/// <summary>
/// Checks if the specified sound is playing.
/// </summary>
/// <param name="soundID">The sound</param>
/// <returns>True if the sound is playing</returns>
        public bool IsPlaying(int soundID)
        {
            Info info = _table[soundID];
            ALSourceState state = AlGetState(info.SourceID);
            return state == ALSourceState.Playing;
        }

        /// <summary>
        /// Checks if the specified buffer is processed.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <returns>True if the sound is processed</returns>
        public bool IsReadyForBuffer(int soundID)
        {
            Info info = _table[soundID];
            int processed;
            AL.GetSource(info.SourceID, ALGetSourcei.BuffersProcessed, out processed);
            ALAnnounceError("AL.GetSource BuffersProcessed");
            return processed != 0;
        }

        /// <summary>
        /// Checks if the specified buffer is stopped.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <returns>True if the sound is stopped</returns>
        public bool IsStopped(int soundID)
        {
            Info info = _table[soundID];
            ALSourceState state = AlGetState(info.SourceID);
            return state == ALSourceState.Stopped;
        }

        /// <summary>
        /// Pauses the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        public void Pause(int soundID)
        {
            Info info = _table[soundID];
            ALPause(info.SourceID);
            info.ShouldBePlaying = false;
        }

        /// <summary>
        /// Plays the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="initialBuffers">First chunk of sound data</param>
        public void Play(int soundID, List<ShortBuffer> initialBuffers)
        {
            if (initialBuffers.Count != MaxQueuedBuffers)
                throw new ArgumentException($"initialBuffers should have length {MaxQueuedBuffers.ToString()} but have length {initialBuffers.Count.ToString()}");

            Info info = _table[soundID];
            if (info.QueuedBufferIDs == null)
            { // this is the first time we are initializing the buffers
                info.QueuedBufferIDs = new List<int>(MaxQueuedBuffers);
                foreach (ShortBuffer buffer in initialBuffers)
                {
                    int bid = AL.GenBuffer();
                    ALAnnounceError("AL.GenBuffer");
                    info.QueuedBufferIDs.Add(bid);
                    AssociateAndQueueBuffer(info, bid, buffer.Data, buffer.Filled);
                } // end for the number of max queued buffers
            }
            else // the buffers had already been filled once, clear the old ones and requeue them with the new data
            {
                // we have to call stop in order to mark all the queued buffers as processed so we
                // can dequeue them.
                ALStop(info.SourceID);
                for (int i = 0; i < MaxQueuedBuffers; i++)
                {
                    AL.SourceUnqueueBuffer(info.SourceID);
                    ALAnnounceError("AL.SourceUnqueueBuffer");
                } // end for the number of max queued buffers
                  // now that all the buffers are dequeued, we can requeue them using the same
                  // buffer IDs.
                for (int i = 0; i < MaxQueuedBuffers; i++)
                {
                    ShortBuffer buffer = initialBuffers[i];
                    int bid = info.QueuedBufferIDs[i];
                    AssociateAndQueueBuffer(info, bid, buffer.Data, buffer.Filled);
                } // end for the number of max queued buffers
            }

            if (_effectSlot != 0)
                BindSourceToAuxiliarysend(soundID);

            ALPlay(info.SourceID);
            info.ShouldBePlaying = true;
        }

        /// <summary>
        /// Prepares a buffer with sound data for playing.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="buffer">The buffer to be queued</param>
        public void QueueBuffer(int soundID, ShortBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentException("buffer cannot be null");

            Info info = _table[soundID];
            // we assume that our caller ran the IsReadyForBuffer method to ensure we are ready for
            // a new buffer.
            int bid = AL.SourceUnqueueBuffer(info.SourceID);
            ALAnnounceError("AL.SourceUnqueueBuffer");
            AssociateAndQueueBuffer(info, bid, buffer.Data, buffer.Filled);
            // check for buffer underruns
            if (buffer.Filled > 0 && info.ShouldBePlaying && AlGetState(info.SourceID) == ALSourceState.Stopped)
            { // the sound has stopped, but we are still receiving buffers with data and our caller's last command was to play, so we must have had a buffer underrun.
                ALPlay(info.SourceID);
            }
        }

        /// <summary>
        /// Resets parameters of the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="channels">Number of channels</param>
        /// <param name="sampleRate">Sample rate of the sound</param>
        public void ReconfigureSound(int soundID, int channels, int sampleRate)
        {
            Info info = _table[soundID];
            info.Channels = channels;
            info.SampleRate = sampleRate;
        }

        /// <summary>
        /// Changes EAX reverb parameters.
        /// </summary>
        /// <param name="reverb">The reverb setting</param>
        /// <param name="name">Name of a preset for text output (optional)</param>
        public void SetEaxReverbProperties(EfxEaxReverb reverb, string name = null)
        {
            // Load effect properties
            _alContext.Suspend();
            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbDensity, reverb.Density);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.Density)}: {reverb.Density.ToString("0.0000")}");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbDiffusion, reverb.Diffusion);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.Diffusion)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbGain, reverb.Gain);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.Gain)}. ");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbGainHF, reverb.GainHF);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.GainHF)}. ");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbGainLF, reverb.GainLF);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.GainLF)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbDecayTime, reverb.DecayTime);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.DecayTime)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbDecayHFRatio, reverb.DecayHFRatio);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.DecayHFRatio)}: {reverb.DecayHFRatio.ToString("0.00")}");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbDecayLFRatio, reverb.DecayLFRatio);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.DecayLFRatio)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbReflectionsGain, reverb.ReflectionsGain);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ReflectionsGain)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbReflectionsDelay, reverb.ReflectionsDelay);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ReflectionsDelay)}.");

            _effectExtension.Effect(_effectHandle, EfxEffect3f.EaxReverbReflectionsPan, ref reverb.ReflectionsPan);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ReflectionsPan)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbLateReverbGain, reverb.LateReverbGain);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.LateReverbGain)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbLateReverbDelay, reverb.LateReverbDelay);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.LateReverbDelay)}.");

            _effectExtension.Effect(_effectHandle, EfxEffect3f.EaxReverbLateReverbPan, ref reverb.LateReverbPan);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.LateReverbPan)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbEchoTime, reverb.EchoTime);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.EchoTime)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbEchoDepth, reverb.EchoDepth);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.EchoDepth)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbModulationTime, reverb.ModulationTime);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ModulationTime)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbModulationDepth, reverb.ModulationDepth);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ModulationDepth)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbAirAbsorptionGainHF, reverb.AirAbsorptionGainHF);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.AirAbsorptionGainHF)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbHFReference, reverb.HFReference);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.HFReference)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbLFReference, reverb.LFReference);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.LFReference)}.");

            _effectExtension.Effect(_effectHandle, EfxEffectf.EaxReverbRoomRolloffFactor, reverb.RoomRolloffFactor);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.RoomRolloffFactor)}.");

            _effectExtension.Effect(_effectHandle, EfxEffecti.EaxReverbDecayHFLimit, (int)reverb.DecayHFLimit);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.DecayHFLimit)}.");

            _alContext.Process();
            _effectExtension.AuxiliaryEffectSlot(_effectSlot, EfxAuxiliaryi.EffectslotEffect, _effectHandle);
            ALAnnounceError($"Apply changes to effect slot.");

            if (!string.IsNullOrEmpty(name))
                Say(name);
        }

        /// <summary>
///Changes the listener orientation.
        /// </summary>
        /// <param name="at">The at vector</param>
        /// <param name="up">The up vector</param>
        public void SetListenerOrientation(Vector3 at, Vector3 up)
        {
            // the defaults for a fresh OpenAL context are: at(0, 0, -1), up(0, 1, 0)
            Vector3 oAt = at.AsOpenTKV3();
            Vector3 oUp = up.AsOpenTKV3();
            AL.Listener(ALListenerfv.Orientation, ref oAt, ref oUp);
            ALAnnounceError("set listener orientation");
        }

        /// <summary>
        /// Changes the listener position.
        /// </summary>
        /// <param name="position">New position</param>
        public void SetListenerPosition(Vector3 position)
        {
            Vector3 otkPos = position.AsOpenTKV3();
            AL.Listener(ALListener3f.Position, ref otkPos);
            ALAnnounceError("set listener position");
        }

        /// <summary>
        /// Sets the listener velocity.
        /// </summary>
        /// <param name="velocity">New velocity</param>
        public void SetListenerVelocity(Vector3 velocity)
          => AL.Listener(ALListener3f.Velocity, velocity.X, velocity.Y, velocity.Z);

        /// <summary>
        /// Sets position of the specified sound source.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="position">New position of the sound</param>
        public void SetPosition(int soundID, Vector3 position)
        {
            Info info = _table[soundID];
            ALSetPosition(info.SourceID, position.AsOpenTKV3());
        }

        /// <summary>
        /// Sets volume of the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="volume">The new volume</param>
        public void SetVolume(int soundID, float volume)
        {
            Info info = _table[soundID];
            ALSetVolume(info.SourceID, volume);
        }

        /// <summary>
Stops the specified sound.
            /// </summary>
        /// <param name="soundID">The sound</param>
        public void Stop(int soundID)
        {
            Info info = _table[soundID];
            ALStop(info.SourceID);
            info.ShouldBePlaying = false;
        }

        /// <summary>
        /// Resumes a previously paused sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        public void Unpause(int soundID)
        {
            Info info = _table[soundID];

            if (info.QueuedBufferIDs == null)
                return;

            if (info.QueuedBufferIDs == null)
                throw Exception("Called Unpause on a sound that had never been played");

            ALPlay(info.SourceID);
            info.ShouldBePlaying = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="args"></param>
        private void ALAnnounceError(string prefix, params object[] args)
        {
            ALError error = AL.GetError();
            if (error != ALError.NoError)
            {
                    string text = error + ", " + AL.GetErrorString(error);
                if (args.Length > 0)
                    text = String.Format(prefix, args) + " " + text;
                else
                    text = prefix + " " + text;

                Say(text);
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceID"></param>
        private void AlDeleteSource(int sourceID)
        {
            AL.DeleteSource(sourceID);
            ALAnnounceError("AL.DeleteSource");
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        private ALSourceState AlGetState(int sourceID)
        {
            int state;
            AL.GetSource(sourceID, ALGetSourcei.SourceState, out state);
            ALAnnounceError("AL.GetSource");
            return (ALSourceState)state;
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceID"></param>
        private void ALPause(int sourceID)
        {
            AL.SourcePause(sourceID);
            ALAnnounceError("AL.SourcePause");
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceID"></param>
        private void ALPlay(int sourceID)
        {
            AL.SourcePlay(sourceID);
            ALAnnounceError("AL.SourcePlay");
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="position"></param>
        private void ALSetPosition(int sourceID, OpenTK.Vector3 position)
        {
            AL.Source(sourceID, ALSource3f.Position, ref position);
            ALAnnounceError("SetPosition");
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="value"></param>
        private void ALSetVolume(int sourceID, float value)
        {
            AL.Source(sourceID, ALSourcef.Gain, value);
            ALAnnounceError("set gain to {0}", value);
        }

        /// <summary>
        /// </summary>
        /// <param name="sourceID"></param>
        private void ALStop(int sourceID)
        {
            AL.SourceStop(sourceID);
            ALAnnounceError("AL.SourceStop");
        }

        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="bufferID"></param>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        private void AssociateAndQueueBuffer(Info info, int bufferID, short[] buffer, int length)
        {
            ALFormat format = info.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
            AL.BufferData(bufferID, format, buffer, length * 2, info.SampleRate);
            ALAnnounceError("AL.BufferData");
            AL.SourceQueueBuffer(info.SourceID, bufferID);
            ALAnnounceError("AL.SourceQueueBuffer");
        }

        private void ValidateReverbPreset(ref EfxEaxReverb r)
        {
            void Validate(ref float parameter, float min, float max)
            {
                parameter = Math.Max(parameter, min);
                parameter = Math.Min(parameter, max);
            }

            Validate(ref r.Density, MinDensity, MaxDensity);
            Validate(ref r.LFReference, MinLFReference, MaxLFReference);
            Validate(ref r.HFReference, MinHFReference, MaxHFReference);
            Validate(ref r.AirAbsorptionGainHF, MinAirAbsorptionGainHF, MaxAirAbsorptionGainHF);
            Validate(ref r.ModulationDepth, MinModulationDepth, MaxModulationDepth);
            Validate(ref r.ModulationTime, MinModulationTime, MaxModulationTime);
            Validate(ref r.EchoDepth, MinEchoDepth, MaxEchoDepth);
            Validate(ref r.EchoTime, MinEchoTime, MaxEchoTime);
            Validate(ref r.LateReverbDelay, MinLateReverbDelay, MaxLateReverbDelay);
            Validate(ref r.RoomRolloffFactor, MinRoomRolloffFactor, MaxRoomRolloffFactor);
            Validate(ref r.LateReverbGain, MinLateReverbGain, MaxLateReverbGain);
            Validate(ref r.ReflectionsDelay, MinReflectionsDelay, MaxReflectionsDelay);
            Validate(ref r.ReflectionsGain, MinReflectionsGain, MaxReflectionsGain);
            Validate(ref r.DecayLFRatio, MinDecayLFRatio, MaxDecayLFRatio);
            Validate(ref r.DecayHFRatio, MinDecayHFRatio, MaxDecayHFRatio);
            Validate(ref r.DecayTime, MinDecayTime, MaxDecayTime);
            Validate(ref r.GainLF, MinGainLF, MaxGainLF);
            Validate(ref r.GainHF, MinGainHF, MaxGainHF);
            Validate(ref r.Gain, MinGain, MaxGain);
            Validate(ref r.Diffusion, MinDiffusion, MaxDiffusion);
        }

        /// <summary>
        /// </summary>
        private sealed class Info
        {
            public int Channels;
            public List<int> QueuedBufferIDs;
            public int SampleRate;
            public bool ShouldBePlaying;
            public int SourceID;
        } // cls
    } // cls
}