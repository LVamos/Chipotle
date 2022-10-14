using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using System;
using System.Collections.Generic;

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
        /// Deactivates lowpass filter on the specified source.
        /// </summary>
        /// <param name="sourceID">Handle of the source to be affected</param>
        public void CancelLowpass(int sourceID)
        {
            int id = _table[sourceID].SourceID;
            AL.Source(id, ALSourcei.EfxDirectFilter, 0);
            ALAnnounceError("Cancelling lowpass filter ona source");
        }

        /// <summary>
        /// Applies the lowpass filter effect on the specified source.
        /// </summary>
        /// <param name="sourceID">Handle of the source to be affected</param>
        /// <param name="gain">Gain of the lowpass parameter</param>
        /// <param name="gainHF">Gain of higher frequencies</param>
        /// <exception cref="InvalidOperationException">Raised when filter creation fails</exception>
        public void ApplyLowpassFilter(int sourceID, (float gain, float gainHF) parameters)
        {
            if (!_table.TryGetValue(sourceID, out Info i))
                return;
            int id = i.SourceID;

            if (_lowpassFilter == 0)
                _lowpassFilter = _efx.GenFilter();
            ALAnnounceError("Creating lowpass filter");

            if (!_efx.IsFilter(_lowpassFilter))
                throw new InvalidOperationException("Filter creation failed");

            _efx.Filter(_lowpassFilter, EfxFilteri.FilterType, (int)EfxFilterType.Lowpass);
            ALAnnounceError("Setting filter type to lowpass");

            _efx.Filter(_lowpassFilter, EfxFilterf.LowpassGain, parameters.gain);
            ALAnnounceError("Setting lowpass gain");

            _efx.Filter(_lowpassFilter, EfxFilterf.LowpassGainHF, parameters.gainHF);
            ALAnnounceError("setting lowpass gainHF");

            AL.Source(id, ALSourcei.EfxDirectFilter, _lowpassFilter);
            ALAnnounceError("Binding lowpass filter to a source");
        }

        /// <summary>
        /// Removes the currently applied filter from the specified source.
        /// </summary>
        /// <param name="id">Handle of the source</param>
        public void DisableFilters(int id)
        {
            AL.Source(_table[id].SourceID, ALSourcei.EfxDirectFilter, 0);
        }

        /// <summary>
        /// Maximum allowed number of buffers filled in the same time
        /// </summary>
        public const int MaxQueuedBuffers = 15;

        /// <summary>
        /// The up vector of listener orientation
        /// </summary>

        /// <summary>
        /// slot used for effect binding
        /// </summary>
        private static int _effectSlot;

        /// <summary>
        /// Audio context used for communication with the audio device
        /// </summary>
        private readonly AudioContext _alContext;

        // Maps soundIDs to their associated info.
        private readonly Dictionary<int, Info> _table = new Dictionary<int, Info>();

        /// <summary>
        /// A delegate used for text output
        /// </summary>
        private readonly Action<string> Say;

        /// <summary>
        /// Instance of the effect extension
        /// </summary>
        private EffectsExtension _efx;

        /// <summary>
        /// Handle of an reverb effect
        /// </summary>
        private int _reverb;
        private int _lowpassFilter;

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
            EffectsExtension.GetEaxFromEfxEax(ref eaxReverb, out EfxEaxReverb efxReverb);
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
            _efx = new EffectsExtension();
            ALAnnounceError("Creating instance of EffectsExtension.");

            _reverb = _efx.GenEffect();
            ALAnnounceError("Geneffect.");

            _efx.BindEffect(_reverb, EfxEffectType.EaxReverb);
            ALAnnounceError("efx.BindEffect");

            _effectSlot = _efx.GenAuxiliaryEffectSlot();
            ALAnnounceError("Creating effect slot.");
        }

        /// <summary>
        /// Initializes a sound stream.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="stream">The stream to be read</param>
        /// <param name="looping">Specifies if looping is enabled or disabled</param>
        /// <param name="channels">Number of channels of the sound</param>
        /// <param name="sampleRate">Sample rate of the sound</param>
        public void InitSound(int soundID, int channels, int sampleRate, PositionType pt, Vector3 position, float frequencyMultiplier)
        {
            Info info = new Info
            {
                Channels = channels,
                SampleRate = sampleRate,
                SourceID = AL.GenSource()
            };
            ALAnnounceError("AL.GenSource");

            AL.Source((uint)info.SourceID, ALSource3i.EfxAuxiliarySendFilter, _effectSlot, 0, 0);

            if (pt == PositionType.None)
            {
                // Set the source to have relative coordinates so it will always play in the center.
                // this is apparently required even when using the direct channels extension.
                AL.Source(info.SourceID, ALSourceb.SourceRelative, true);
                ALAnnounceError("setting source relative to true");

                AL.Source(info.SourceID, ALSourceb.EfxDirectFilterGainHighFrequencyAuto, true); // using the OpenAL Soft direct channels extension to get rid of sound coloring that occurs when HRTF is enabled.
                ALAnnounceError("setting direct channels to true");
            }
            else if (pt == PositionType.Relative)
            {
                AL.Source(info.SourceID, ALSourceb.SourceRelative, true);
                ALAnnounceError("setting source relative to true");

                ALSetPosition(info.SourceID, position);
            }
            else if (pt == PositionType.Absolute)
            {
                ALSetPosition(info.SourceID, position);
                AL.DistanceModel(ALDistanceModel.LinearDistance);
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
            if (_table.TryGetValue(soundID, out Info info))
                return AlGetState(info.SourceID) == ALSourceState.Playing;
            return false;
        }

        /// <summary>
        /// Checks if the specified buffer is processed.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <returns>True if the sound is processed</returns>
        public bool IsReadyForBuffer(int soundID)
        {
            Info info = _table[soundID];
            AL.GetSource(info.SourceID, ALGetSourcei.BuffersProcessed, out int processed);
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
        /// Changes the EAX reverb reflections gain parameter
        /// </summary>
        /// <param name="value">The reflections gain value</param>
        public void SetEaxReverbReflectionsGain(float value)
        {
            _efx.Effect(_reverb, EfxEffectf.EaxReverbReflectionsGain, value);
            ALAnnounceError($"Setting reverb properties: {nameof(value)}.");
        }

        /// <summary>
        /// Plays the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="initialBuffers">First chunk of sound data</param>
        public void Play(int soundID, List<ShortBuffer> initialBuffers)
        {

            if (initialBuffers.Count != MaxQueuedBuffers)
                throw new ArgumentException($"initialBuffers should have length {MaxQueuedBuffers} but have length {initialBuffers.Count}");

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
        /// Changes value of reflections parameter of EAX reverb.
        /// </summary>
        /// <param name="pan">The reflections pan vector</param>
        public void SetEaxReverbReflectionsPan(Vector3 pan)
        {
            _efx.Effect(_reverb, EfxEffect3f.EaxReverbReflectionsPan, ref pan);
            ALAnnounceError($"Setting reverb properties: {nameof(pan)}.");
        }

        /// <summary>
        /// Changes EAX reverb parameters.
        /// </summary>
        /// <param name="reverb">The reverb setting</param>
        /// <param name="name">Name of a preset for text output (optional)</param>
        public void SetEaxReverbProperties(EfxEaxReverb reverb, string name = null)
        {
            // Load effect properties
            _efx.Effect(_reverb, EfxEffectf.EaxReverbDensity, reverb.Density);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.Density)}: {reverb.Density:0.0000}");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbDiffusion, reverb.Diffusion);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.Diffusion)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbGain, reverb.Gain);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.Gain)}. ");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbGainHF, reverb.GainHF);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.GainHF)}. ");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbGainLF, reverb.GainLF);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.GainLF)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbDecayTime, reverb.DecayTime);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.DecayTime)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbDecayHFRatio, reverb.DecayHFRatio);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.DecayHFRatio)}: {reverb.DecayHFRatio:0.00}");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbDecayLFRatio, reverb.DecayLFRatio);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.DecayLFRatio)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbReflectionsGain, reverb.ReflectionsGain);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ReflectionsGain)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbReflectionsDelay, reverb.ReflectionsDelay);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ReflectionsDelay)}.");

            _efx.Effect(_reverb, EfxEffect3f.EaxReverbReflectionsPan, ref reverb.ReflectionsPan);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ReflectionsPan)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbLateReverbGain, reverb.LateReverbGain);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.LateReverbGain)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbLateReverbDelay, reverb.LateReverbDelay);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.LateReverbDelay)}.");

            _efx.Effect(_reverb, EfxEffect3f.EaxReverbLateReverbPan, ref reverb.LateReverbPan);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.LateReverbPan)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbEchoTime, reverb.EchoTime);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.EchoTime)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbEchoDepth, reverb.EchoDepth);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.EchoDepth)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbModulationTime, reverb.ModulationTime);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ModulationTime)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbModulationDepth, reverb.ModulationDepth);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.ModulationDepth)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbAirAbsorptionGainHF, reverb.AirAbsorptionGainHF);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.AirAbsorptionGainHF)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbHFReference, reverb.HFReference);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.HFReference)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbLFReference, reverb.LFReference);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.LFReference)}.");

            _efx.Effect(_reverb, EfxEffectf.EaxReverbRoomRolloffFactor, reverb.RoomRolloffFactor);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.RoomRolloffFactor)}.");

            _efx.Effect(_reverb, EfxEffecti.EaxReverbDecayHFLimit, (int)reverb.DecayHFLimit);
            ALAnnounceError($"Setting reverb properties: {nameof(reverb.DecayHFLimit)}.");

            _efx.AuxiliaryEffectSlot(_effectSlot, EfxAuxiliaryi.EffectslotEffect, _reverb);
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
            Vector3 oAt = at;
            Vector3 oUp = up;
            AL.Listener(ALListenerfv.Orientation, ref oAt, ref oUp);
            ALAnnounceError("set listener orientation");
        }

        /// <summary>
        /// Changes the listener position.
        /// </summary>
        /// <param name="position">New position</param>
        public void SetListenerPosition(Vector3 position)
        {
            Vector3 otkPos = position;
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
        public void SetPosition(int soundID, Vector3 position, PositionType type = PositionType.Absolute)
        {
            Info info = _table[soundID];

            AL.Source(info.SourceID, ALSourceb.SourceRelative, type == PositionType.Relative);
            ALSetPosition(info.SourceID, position);
        }

        /// <summary>
        /// Sets volume of the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="volume">The new volume</param>
        public void SetVolume(int soundID, float volume)
        {
            if (_table.TryGetValue(soundID, out Info info))
                ALSetVolume(info.SourceID, volume);
        }

        /// <summary>
        /// Stops the specified sound.
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
                throw new Exception("Called Unpause on a sound that had never been played");

            ALPlay(info.SourceID);
            info.ShouldBePlaying = true;
        }

        /// <summary>
        /// Checks for OpenAl error and announces it.
        /// </summary>
        /// <param name="prefix">Short description of the action performed before the error check</param>
        /// <param name="args">Details of the action</param>
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

                throw new InvalidOperationException(text);
            }
        }

        /// <summary>
        /// Deletes a sound source.
        /// </summary>
        /// <param name="sourceID">The sound source</param>
        private void AlDeleteSource(int sourceID)
        {
            AL.DeleteSource(sourceID);
            ALAnnounceError("AL.DeleteSource");
        }

        /// <summary>
        /// Returns current state of the specified sound source.
        /// </summary>
        /// <param name="sourceID">The sound source</param>
        /// <returns>The state</returns>
        private ALSourceState AlGetState(int sourceID)
        {
            AL.GetSource(sourceID, ALGetSourcei.SourceState, out int state);
            ALAnnounceError("AL.GetSource");
            return (ALSourceState)state;
        }

        /// <summary>
        /// Pauses the specified sound source.
        /// </summary>
        /// <param name="sourceID">The sound source</param>
        private void ALPause(int sourceID)
        {
            AL.SourcePause(sourceID);
            ALAnnounceError("AL.SourcePause");
        }

        /// <summary>
        /// Plays the specified sound source.
        /// </summary>
        /// <param name="sourceID">The sound source</param>
        private void ALPlay(int sourceID)
        {
            AL.SourcePlay(sourceID);
            ALAnnounceError("AL.SourcePlay");
        }

        /// <summary>
        /// Changes position of the specified sound source.
        /// </summary>
        /// <param name="sourceID">The sound source</param>
        /// <param name="position">New position</param>
        private void ALSetPosition(int sourceID, OpenTK.Vector3 position)
        {
            AL.Source(sourceID, ALSource3f.Position, ref position);
            ALAnnounceError("SetPosition");
        }

        /// <summary>
        /// Sets volume of the specified sound source.
        /// </summary>
        /// <param name="sourceID">The sound source</param>
        /// <param name="volume">New volume</param>
        private void ALSetVolume(int sourceID, float volume)
        {
            AL.Source(sourceID, ALSourcef.Gain, volume);
            ALAnnounceError("set gain to {0}", volume);
        }

        /// <summary>
        /// Stops the specified sound source.
        /// </summary>
        /// <param name="sourceID">The sound source</param>
        private void ALStop(int sourceID)
        {
            AL.SourceStop(sourceID);
            ALAnnounceError("AL.SourceStop");
        }

        /// <summary>
        /// Associates sound data with a buffer and queues the buffer for playing.
        /// </summary>
        /// <param name="info">Sound parameters</param>
        /// <param name="bufferID">A buffer identifier</param>
        /// <param name="buffer">The buffer to be queued</param>
        /// <param name="length">Amount of the data to be read</param>
        private void AssociateAndQueueBuffer(Info info, int bufferID, short[] buffer, int length)
        {
            ALFormat format = info.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
            AL.BufferData(bufferID, format, buffer, length * 2, info.SampleRate);
            ALAnnounceError("AL.BufferData");
            AL.SourceQueueBuffer(info.SourceID, bufferID);
            ALAnnounceError("AL.SourceQueueBuffer");
        }

        /// <summary>
        /// Validates a EAX reverb preset and corrects extreme values.
        /// </summary>
        /// <param name="r">The preset to be validated</param>
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
        /// Stores information about a stream.
        /// </summary>
        private sealed class Info
        {
            /// <summary>
            /// Number of channels
            /// </summary>
            public int Channels;

            /// <summary>
            /// List of handles to queued buffers
            /// </summary>
            public List<int> QueuedBufferIDs;

            /// <summary>
            /// Sample rate of the MP3 file
            /// </summary>
            public int SampleRate;

            /// <summary>
            /// starts or stops playback of the sound.
            /// </summary>
            public bool ShouldBePlaying;

            /// <summary>
            /// Identifier of the sound
            /// </summary>
            public int SourceID;
        }
    }
}