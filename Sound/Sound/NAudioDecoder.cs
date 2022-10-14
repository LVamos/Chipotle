using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Luky
{
    /// <summary>
    /// Decodes MP3 sound files.
    /// </summary>
    internal sealed class NAudioDecoder : DebugSO, IDecoder
    {
        /// <summary>
        /// Specifies how many bytes are needed per one sample.
        /// </summary>
        private const int _bytesPerSample = 2;

        /// <summary>
        /// A buffer used for sound stream reading
        /// </summary>
        private readonly byte[] _buffer = new byte[3840];

        /// <summary>
        /// Maps soundIDs to their associated info.
        /// </summary>
        private readonly Dictionary<int, Info> _table = new Dictionary<int, Info>();

        /// <summary>
        /// Disposes the decoder.
        /// </summary>
        public void Dispose()
        { // dispose each sound
            foreach (int soundID in _table.Keys.ToArray())
                DisposeStream(soundID);
        }

        /// <summary>
        /// Disposes the specified stream.
        /// </summary>
        /// <param name="soundID">ID of the sound stream to be disposed</param>
        public void DisposeStream(int soundID)
        {
            Info info = _table[soundID];
            _table.Remove(soundID);
            info.Reader.Dispose();
            info.Stream.Dispose();
        }

        /// <summary>
        /// Loads sound data from the specified stream to the specified buffer.
        /// </summary>
        /// <param name="soundID">ID of the sound to be read</param>
        /// <param name="buffer">The buffer to be filled</param>
        /// <returns>the number of shorts written to the buffer</returns>
        public int FillBuffer(int soundID, short[] buffer)
        {
            Info info = _table[soundID];
            int bytesRead = info.Reader.Read(_buffer, 0, _buffer.Length);

            if (bytesRead == 0)
            {
                // If looping is disabled, it can be stopped, otherwise it seeks to the beginning of
                // the stream.
                if (!info.Looping)
                    return 0; // nothing to read.
                else
                {
                    // It seeks to the beginning of the stream and tries reading again.
                    info.Reader.Dispose();
                    info.Stream.Seek(0, SeekOrigin.Begin);
                    info.Reader = new StreamMediaFoundationReader(info.Stream);
                    bytesRead = info.Reader.Read(_buffer, 0, _buffer.Length);
                }
            }

            int shortsRead = bytesRead / 2;

            if (buffer.Length < shortsRead)
                throw new InvalidOperationException($"buffer is too small, has length {buffer.Length} but needs length {shortsRead}");

            System.Buffer.BlockCopy(_buffer, 0, buffer, 0, bytesRead);

            if (info.ForceMono && info.Channels == 2)
                shortsRead = OpusFileDecoder.ConvertStereoToMono(buffer, shortsRead);

            return shortsRead;
        }

        /// <summary>
        /// Optains current position of the specified sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="currentSample"></param>
        public void GetDynamicInfo(int soundID, out int currentSample)
        {
            Info info = _table[soundID];
            currentSample = (int)info.Reader.Position / _bytesPerSample / info.Channels;
        }

        /// <summary>
        /// Optains static information about the specified sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="sampleRate">Reference to a variable to store sample rate of the sound</param>
        /// <param name="totalSamples">Reference to a variable to store length of the sound</param>
        /// <param name="channels">Reference to a variable to store number of channels of the sound</param>
        public void GetStaticInfo(int soundID, out int sampleRate, out int totalSamples, out int channels)
        {
            Info info = _table[soundID];
            sampleRate = info.Reader.WaveFormat.SampleRate;
            totalSamples = (int)info.Reader.Length / _bytesPerSample / info.Channels;
            channels = info.Channels;
        }

        /// <summary>
        /// Enables or disables looping.
        /// </summary>
        /// <param name="soundID">ID of the sound to be modified</param>
        /// <param name="looping">Specifies if looping is enabled or disabled</param>
        public void ChangeLooping(int soundID, bool looping)
        {
            Info info = _table[soundID];
            info.Looping = looping;
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
        public void InitStream(int soundID, Stream stream, bool looping, bool forceMono, out int channels, out int sampleRate)
        {
            Info info = new Info
            {
                Stream = stream,
                Looping = looping,
                ForceMono = forceMono,
                Reader = new StreamMediaFoundationReader(stream)
            };
            sampleRate = info.Reader.WaveFormat.SampleRate;
            info.Channels = info.Reader.WaveFormat.Channels;
            channels = info.Channels;

            if (forceMono && channels == 2)
                channels = 1;

            _table[soundID] = info;
        }

        /// <summary>
        /// Jumps to a sample of the sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="seekSample">Number of the sample to jump to</param>
        public void SeekToSample(int soundID, int seekSample)
        {
            Info info = _table[soundID];
            info.Reader.Position = seekSample * _bytesPerSample * info.Channels;
        }

        /// <summary>
        /// Stores information about a stream.
        /// </summary>
        private sealed class Info
        {
            /// <summary>
            /// Specifies if the sound should be converted to mono
            /// </summary>
            public bool ForceMono;

            /// <summary>
            /// Number of channels
            /// </summary>
            public int Channels;

            /// <summary>
            /// Specifies if looping is enabled or disabled
            /// </summary>
            public bool Looping;

            /// <summary>
            /// MP3 file reader
            /// </summary>
            public StreamMediaFoundationReader Reader;

            /// <summary>
            /// A stream used to read from files
            /// </summary>
            public Stream Stream;
        }
    }
}