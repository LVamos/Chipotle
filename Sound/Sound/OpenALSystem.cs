using static Sound.EaxReverbDefaults;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

using Sound;

using System;
using System.Collections.Generic;

using EaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EaxReverb;
using EfxEaxReverb = OpenTK.Audio.OpenAL.EffectsExtension.EfxEaxReverb;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class OpenALSystem : DebugSO, IPlayback
    {
        public void BindSourceToAuxiliarysend(int soundId)
        {
            if (_effectSlot != 0)
                AL.Source(_table[soundId].SourceID, ALSource3i.EfxAuxiliarySendFilter, _effectSlot, 0, 0);
        }

        public void ApplyEaxReverbPreset(EaxReverb preset, string name = null, float gain=0)
        {
            EaxReverb eaxReverb = preset;
            EfxEaxReverb efxReverb;
            EffectsExtension.GetEaxFromEfxEax(ref eaxReverb, out efxReverb);
            ValidateReverbPreset(ref efxReverb);
                efxReverb.Gain = gain;
            SetEaxReverbProperties(efxReverb, name);
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

        public void SetEaxReverbProperties(EfxEaxReverb reverb, string name = null)
        {
            // Load effect properties
            Alc.SuspendContext(_alContext.context_handle);
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

            Alc.ProcessContext(_alContext.context_handle);
            _effectExtension.AuxiliaryEffectSlot(_effectSlot, EfxAuxiliaryi.EffectslotEffect, _effectHandle);
            ALAnnounceError($"Apply changes to effect slot.");


            if (!string.IsNullOrEmpty(name))
                DebugSO.SayDelegate(name);
        }


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

        // each Opus buffer is 20ms of data, so we generally have between 70 and 100ms of unplayed buffer data, lagging up to 20ms for the buffer that is currently playing, and up to 10ms for the rate at which OnTick checks.
        // we use buffers of type short, of length equal to a 20ms Opus packet.
        // that is 960 shorts for mono, 1920 shorts for stereo, because 48000Hz per second is 960Hz per 20ms.
        // buffering 100ms uses 9600 bytes for mono and 19200 bytes for stereo, or roughly 10K and 20K respectively.
        // 10 buffers gives us 200ms of buffer, at 20K for mono and 40K for stereo.
        public const int MaxQueuedBuffers = 5; // 2 consistently gets buffer underruns, 3 seems fine for 48000Hz sounds, but gets underruns for 96000Hz sounds. 5 works most of the time, but when my computer is busy doing something else it gets an underrun.

        private static readonly OpenTK.Vector3 _up = new OpenTK.Vector3(0, 0, 1);

        private static int _effectSlot;

        private AudioContext _alContext;

        // this maps soundIDs to their associated info.
        private Dictionary<int, Info> _table = new Dictionary<int, Info>();
        private EffectsExtension _effectExtension;
        private int _effectHandle;

        /// <summary>
        /// private constructor
        /// </summary>
        private OpenALSystem(AudioContext context)
            => _alContext = context;

        /// <summary>
        /// Initializes OpenAL and starts thread. OpenAL is binded to current thread. Should be used instead of constructor.
        /// </summary>
        /// <param name="useHRTF"></param>
        /// <returns>instance of OpenALSystem</returns>
        public static OpenALSystem CreateAndBindToThisThread(bool? useHRTF)
            => new OpenALSystem(new AudioContext(null, 0, 0, false, true, AudioContext.MaxAuxiliarySends.UseDriverDefault, useHRTF, true));

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        => _alContext.Dispose();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
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
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        /// <param name="pt"></param>
        /// <param name="position"></param>
        /// <param name="frequencyMultiplier"></param>
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
                // we set the source to have relative coordinates so it will always play in the center.
                // this is apparently required even when we are using the direct channels extension.
                AL.Source(info.SourceID, ALSourceb.SourceRelative, true);
                ALAnnounceError("setting source relative to true");
                // using the OpenAL Soft direct channels extension to get rid of sound coloring that occurs when HRTF is enabled.
                AL.Source(info.SourceID, ALSourceb.DirectChannels, true);
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
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <returns></returns>
        public bool IsPlaying(int soundID)
        {
            Info info = _table[soundID];
            ALSourceState state = AlGetState(info.SourceID);
            return state == ALSourceState.Playing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <returns></returns>
        public bool IsReadyForBuffer(int soundID)
        {
            Info info = _table[soundID];
            int processed;
            AL.GetSource(info.SourceID, ALGetSourcei.BuffersProcessed, out processed);
            ALAnnounceError("AL.GetSource BuffersProcessed");
            return processed != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <returns></returns>
        public bool IsStopped(int soundID)
        {
            Info info = _table[soundID];
            ALSourceState state = AlGetState(info.SourceID);
            return state == ALSourceState.Stopped;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        public void Pause(int soundID)
        {
            Info info = _table[soundID];
            ALPause(info.SourceID);
            info.ShouldBePlaying = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="initialBuffers"></param>
        public void Play(int soundID, List<ShortBuffer> initialBuffers)
        {
            if (initialBuffers.Count != MaxQueuedBuffers)
                throw ArgumentException("initialBuffers should have length {0} but have length {1}", MaxQueuedBuffers, initialBuffers.Count);

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
                // we have to call stop in order to mark all the queued buffers as processed so we can dequeue them.
                ALStop(info.SourceID);
                for (int i = 0; i < MaxQueuedBuffers; i++)
                {
                    AL.SourceUnqueueBuffer(info.SourceID);
                    ALAnnounceError("AL.SourceUnqueueBuffer");
                } // end for the number of max queued buffers
                  // now that all the buffers are dequeued, we can requeue them using the same buffer IDs.
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
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="buffer"></param>
        public void QueueBuffer(int soundID, ShortBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentException("buffer cannot be null");

            Info info = _table[soundID];
            // we assume that our caller ran the IsReadyForBuffer method to ensure we are ready for a new buffer.
            int bid = AL.SourceUnqueueBuffer(info.SourceID);
            ALAnnounceError("AL.SourceUnqueueBuffer");
            AssociateAndQueueBuffer(info, bid, buffer.Data, buffer.Filled);
            // check for buffer underruns
            if (buffer.Filled > 0 && info.ShouldBePlaying && AlGetState(info.SourceID) == ALSourceState.Stopped)
            { // the sound has stopped, but we are still receiving buffers with data and our caller's last command was to play, so we must have had a buffer underrun.
                ALPlay(info.SourceID);
                Say("Recovered from buffer underrun");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        public void ReconfigureSound(int soundID, int channels, int sampleRate)
        {
            Info info = _table[soundID];
            info.Channels = channels;
            info.SampleRate = sampleRate;
        }

        public void SetListenerVelocity(Vector3 velocity)
          => AL.Listener(ALListener3f.Velocity, velocity.X, velocity.Y, velocity.Z);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="at"></param>
        /// <param name="up"></param>
        public void SetListenerOrientation(Vector3 at, Vector3 up)
        {
            OpenTK.Vector3 oAt = at.AsOpenTKV3();
            OpenTK.Vector3 oUp = up.AsOpenTKV3();
            // the defaults for a fresh OpenAL context are: at is (0, 0, -1), up is (0, 1, 0)
            // but I set them to this when we startup: at is (0, -1, 0), up is (0, 0, -1)
            AL.Listener(ALListenerfv.Orientation, ref oAt, ref oUp);
            ALAnnounceError("set listener orientation");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public void SetListenerPosition(Vector3 position)
        {
            OpenTK.Vector3 otkPos = position.AsOpenTKV3();
            AL.Listener(ALListener3f.Position, ref otkPos);
            ALAnnounceError("set listener position");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="position"></param>
        public void SetPosition(int soundID, Vector3 position)
        {
            Info info = _table[soundID];
            ALSetPosition(info.SourceID, position.AsOpenTKV3());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="value"></param>
        public void SetVolume(int soundID, float value)
        {
            Info info = _table[soundID];
            ALSetVolume(info.SourceID, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public void SpeakHRTF()
        => Say("HRTF: ", _alContext.GetHRTFEnabled(), _alContext.GetHRTFStatus());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        public void Stop(int soundID)
        {
            Info info = _table[soundID];
            ALStop(info.SourceID);
            info.ShouldBePlaying = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ToggleHRTF()
        {
            bool useHRTF = !_alContext.GetHRTFEnabled();
            _alContext.ResetDevice(0, 0, false, true, AudioContext.MaxAuxiliarySends.UseDriverDefault, useHRTF);
            SpeakHRTF();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        public void Unpause(int soundID)
        {
            Info info = _table[soundID];
            // we could alternatively just do nothing if the sound has never been played before this.
            if (info.QueuedBufferIDs == null)
                return;
            //if (info.QueuedBufferIDs == null)
            //    throw Exception("Called Unpause on a sound that had never been played");
            ALPlay(info.SourceID);
            info.ShouldBePlaying = true;
        }

        /// <summary>
        /// 
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

                SayDelegate(text);
                        System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        private void AlDeleteSource(int sourceID)
        {
            AL.DeleteSource(sourceID);
            ALAnnounceError("AL.DeleteSource");
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        private void ALPause(int sourceID)
        {
            AL.SourcePause(sourceID);
            ALAnnounceError("AL.SourcePause");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        private void ALPlay(int sourceID)
        {
            AL.SourcePlay(sourceID);
            ALAnnounceError("AL.SourcePlay");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="position"></param>
        private void ALSetPosition(int sourceID, OpenTK.Vector3 position)
        {
            AL.Source(sourceID, ALSource3f.Position, ref position);
            ALAnnounceError("SetPosition");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="value"></param>
        private void ALSetVolume(int sourceID, float value)
        {
            AL.Source(sourceID, ALSourcef.Gain, value);
            ALAnnounceError("set gain to {0}", value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        private void ALStop(int sourceID)
        {
            AL.SourceStop(sourceID);
            ALAnnounceError("AL.SourceStop");
        }

        /// <summary>
        /// 
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

        /// <summary>
        /// 
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