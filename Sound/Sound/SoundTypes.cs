using OpenTK;

using System.Collections.Generic;
using System.IO;

namespace Luky
{
    /// <summary>
    /// Defines available sound decoders.
    /// </summary>
    public enum Decoder
    {
        NotSet,
        Opusfile,
        Libsndfile,
        NAudio,
    }

    /// <summary>
    /// Defines playback methods for sound output
    /// </summary>
    public enum Playback
    {
        NotSet,
        OpenAL,
        NAudio,
    }

    /// <summary>
    /// Specifies how the sound position is calculated.
    /// </summary>
    public enum PositionType
    {
        None, // no positioning
        Relative, // relative to the listener
        Absolute
    }

    /// <summary>
    /// Defines states of a sound.
    /// </summary>
    public enum SoundState
    {
        Playing,
        Paused,
        Disposed,
    }

    /// <summary>
    /// An interface for sound asset streams
    /// </summary>
    internal interface IAssetStream
    {
        /// <summary>
        /// Checks if the asset stream can be played.
        /// </summary>
        /// <returns>True if the asset stream can be played</returns>
        bool IsPrimed();
    }

    /// <summary>
    /// Declares required methods for all sound decoders.
    /// </summary>
    internal interface IDecoder
    {
        /// <summary>
        /// Disposes the specified sound stream.
        /// </summary>
        /// <param name="soundID">ID of the sound stream to be disposed</param>
        void DisposeStream(int streamID);

        /// <summary>
        /// Loads sound data from the specified stream to the specified buffer.
        /// </summary>
        /// <param name="soundID">ID of the sound to be read</param>
        /// <param name="buffer">The buffer to be filled</param>
        /// <returns>the number of shorts written to the buffer</returns>
        int FillBuffer(int streamID, short[] buffer);

        /// <summary>
        /// Optains current position of the specified sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="currentSample"></param>
        void GetDynamicInfo(int soundID, out int currentSample);

        /// <summary>
        /// Optains static information about the specified sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="sampleRate">Reference to a variable to store sample rate of the sound</param>
        /// <param name="totalSamples">Reference to a variable to store length of the sound</param>
        /// <param name="channels">Reference to a variable to store number of channels of the sound</param>
        void GetStaticInfo(int soundID, out int sampleRate, out int totalSamples, out int channels);

        /// <summary>
        /// Enables or disables looping.
        /// </summary>
        /// <param name="soundID">ID of the sound to be modified</param>
        /// <param name="looping">Specifies if looping is enabled or disabled</param>
        void ChangeLooping(int soundID, bool looping);

        /// <summary>
        /// Initializes a sound stream.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="stream">The stream to be read</param>
        /// <param name="looping">Specifies if looping is enabled or disabled</param>
        /// <param name="forceMono">Specifies if the sound should be converted to mono</param>
        /// <param name="channels">Reference to a variable to store number of channels of the sound</param>
        /// <param name="sampleRate">Reference to a variable to store sample rate of the sound</param>
        void InitStream(int streamID, Stream stream, bool looping, bool forceMono, out int channels, out int sampleRate);

        /// <summary>
        /// Jumps to a sample of the sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="seekSample">Number of the sample to jump to</param>
        void SeekToSample(int soundID, int seekSample);
    }

    /// <summary>
    /// Declares required methods for all sound players.
    /// </summary>
    internal interface IPlayback
    {
        /// <summary>
        /// Disposes the specified sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        void DisposeSound(int soundID);

        /// <summary>
        /// Initializes a sound stream.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="stream">The stream to be read</param>
        /// <param name="looping">Specifies if looping is enabled or disabled</param>
        /// <param name="forceMono">Specifies if the sound should be converted to mono</param>
        /// <param name="channels">Reference to a variable to store number of channels of the sound</param>
        /// <param name="sampleRate">Reference to a variable to store sample rate of the sound</param>
        void InitSound(int soundID, int channels, int sampleRate, PositionType pt, Vector3 position, float frequencyMultiplier);

        /// <summary>
        /// Checks if the specified sound is playing.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <returns>True if the sound is playing</returns>
        bool IsPlaying(int soundID);

        /// <summary>
        /// Checks if the specified buffer is processed.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <returns>True if the sound is processed</returns>
        bool IsReadyForBuffer(int soundID);

        /// <summary>
        /// Checks if the specified buffer is stopped.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <returns>True if the sound is stopped</returns>
        bool IsStopped(int soundID);

        /// <summary>
        /// Pauses the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        void Pause(int soundID);

        /// <summary>
        /// Plays the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="initialBuffers">First chunk of sound data</param>
        void Play(int soundID, List<ShortBuffer> initialBuffers);

        /// <summary>
        /// Prepares a buffer with sound data for playing.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="buffer">The buffer to be queued</param>
        void QueueBuffer(int soundID, ShortBuffer buffer);

        /// <summary>
        /// Sets volume of the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="volume">The new volume</param>
        void SetVolume(int soundID, float volume);

        /// <summary>
        /// Stops the specified sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        void Stop(int soundID);

        /// <summary>
        /// Resumes a previously paused sound.
        /// </summary>
        /// <param name="soundID">The sound</param>
        void Unpause(int soundID);
    }

    /// <summary>
    /// A sound buffer
    /// </summary>
    public sealed class ShortBuffer
    {
        /// <summary>
        /// The buffer
        /// </summary>
        public readonly short[] Data;

        /// <summary>
        /// The number of shorts actually being used in the array
        /// </summary>
        public int Filled;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="length">Length of the buffer</param>
        public ShortBuffer(int length)
        => Data = new short[length];
    }
}