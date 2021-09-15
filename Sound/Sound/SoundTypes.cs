using System;
using System.Collections.Generic;
using System.IO;

using OpenTK;

namespace Luky
{
    /// <summary>
    /// </summary>
    public enum Playback
    {
        NotSet,
        OpenAL,
        NAudio,
    }

    /// <summary>
    /// </summary>
    public enum PositionType
    {
        None, // no positioning
        Relative, // relative to the listener
        Absolute
    }

    /// <summary>
    /// </summary>
    public enum SoundState
    {
        Playing,
        Paused,
        Disposed,
    }

    /// <summary>
    /// </summary>
    internal enum Decoder
    {
        NotSet,
        Opusfile,
        Libsndfile,
        NAudio,
    }

    /// <summary>
    /// </summary>
    internal interface IAssetStream
    {
        bool IsPrimed();
    }

    /// <summary>
    /// </summary>
    internal interface IDecoder
    {
        void DisposeStream(int streamID);

        int FillBuffer(int streamID, short[] buffer);

        void GetDynamicInfo(int soundID, out int currentSample);

        void GetStaticInfo(int soundID, out int sampleRate, out int totalSamples, out int channels);

        void ChangeLooping(int soundID, bool looping);

        void InitStream(int streamID, Stream stream, bool looping, bool forceMono, out int channels, out int sampleRate);

        void SeekToSample(int soundID, int seekSample);
    }

    /// <summary>
    /// </summary>
    internal interface IPlayback
    {
        void DisposeSound(int soundID);

        void InitSound(int soundID, int channels, int sampleRate, PositionType pt, Vector3 position, float frequencyMultiplier);

        bool IsPlaying(int soundID);

        bool IsReadyForBuffer(int soundID);

        bool IsStopped(int soundID);

        void Pause(int soundID);

        void Play(int soundID, List<ShortBuffer> initialBuffers);

        void QueueBuffer(int soundID, ShortBuffer buffer);

        void SetVolume(int soundID, float value);

        void Stop(int soundID);

        void Unpause(int soundID);
    }

    /// <summary>
    /// </summary>
    public sealed class Snapshot
    {
        internal Dictionary<Name, float> GroupVolumes = new Dictionary<Name, float>();
        internal bool Immutable;

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddGroupVolume(Name name, float value)
        {
            if (Immutable)
                throw new Exception("Attempted to change a snapshot after it had been set as immutable");

            GroupVolumes.Add(name, value);
        }
    }

    /// <summary>
    /// </summary>
    internal sealed class ShortBuffer
    {
        public readonly short[] Data;
        public int Filled; // the number of shorts actually being used in the array.

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="length"></param>
        public ShortBuffer(int length)
        => Data = new short[length];
    }
}