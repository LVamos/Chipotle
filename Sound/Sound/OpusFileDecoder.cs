using OpusfileSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Luky
{
    /// <summary>
    /// Decodes opus sound files.
    /// </summary>
    /// <remarks>Uses OpusFileSharp which is a PInvoke wrapper around libopusfile</remarks>
    internal sealed class OpusFileDecoder : DebugSO, IDecoder
    {
        /// <summary>
        /// Default sample rate for all opus files
        /// </summary>
        private const int _sampleRate = 48000;

        // Maps sound IDs to their associated info.
        private readonly Dictionary<int, Info> _table = new Dictionary<int, Info>();

        /// <summary>
        /// Converts the given sound data to mono.
        /// </summary>
        /// <param name="buffer">The data to be converted</param>
        /// <param name="shortsRead">Length of the data</param>
        /// <returns>the new length</returns>
        public static int ConvertStereoToMono(short[] buffer, int shortsRead)
        {
            if (shortsRead % 2 != 0)
                throw new Exception("read an odd number of bytes");

            int writeIndex = 0;
            for (int i = 0; i < shortsRead; i += 2)
            { // use L/2 + R/2 to combine the stereo channels into mono.
                int combined = (buffer[i] / 2) + (buffer[i + 1] / 2);
                buffer[writeIndex] = (short)combined;
                writeIndex++;
            }
            return shortsRead / 2;
        }

        /// <summary>
        /// Disposes the decoder.
        /// </summary>
        public void Dispose()
        {
            foreach (int soundID in _table.Keys.ToArray())
                DisposeStream(soundID);
        }

        /// <summary>
        /// Disposes the specified sound stream.
        /// </summary>
        /// <param name="soundID">ID of the sound stream to be disposed</param>
        public void DisposeStream(int soundID)
        {
            Info info = _table[soundID];
            _table.Remove(soundID);
            info.OpusFile.Dispose();
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
            int samplesRead = info.OpusFile.Read(buffer, 0, buffer.Length);
            if (samplesRead == 0)
            {
                // If looping is disabled, it can be stopped, otherwise seek to the beginning of the stream.
                if (!info.Looping)
                    return 0; // nothing to read.
                else
                {
                    // Seek to the beginning of the stream and try reading again.
                    info.OpusFile.Dispose();
                    info.Stream.Seek(0, SeekOrigin.Begin);
                    info.OpusFile = new OggOpusFile(info.Stream, true);
                    samplesRead = info.OpusFile.Read(buffer, 0, buffer.Length);
                }
            }

            int shortsRead = samplesRead * info.Channels;

            if (info.ForceMono && info.Channels == 2)
                shortsRead = ConvertStereoToMono(buffer, shortsRead);

            return shortsRead;
        }

        /// <summary>
        /// Returns the buffer associated with the ID.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <returns>The buffer</returns>
        public short[] GetBuffer(int soundID)
        {
            short[] buffer = new short[1920];
            int shortsRead = FillBuffer(soundID, buffer);

            if (shortsRead == buffer.Length)
                return buffer;

            short[] buffer2 = new short[shortsRead];
            System.Buffer.BlockCopy(buffer, 0, buffer2, 0, shortsRead * 2);
            return buffer2;
        }

        /// <summary>
        /// Optains current position of the specified sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="current"></param>
        public void GetDynamicInfo(int soundID, out int current)
        {
            Info info = _table[soundID];
            current = (int)info.OpusFile.PcmTell;
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
            sampleRate = _sampleRate;
            totalSamples = (int)info.OpusFile.GetPcmTotal();
            channels = info.Channels;
        }

        /// <summary>
        /// Sets the parameter that decides if the sound should be converted to mono.
        /// </summary>
        /// <param name="soundID">The sound</param>
        /// <param name="forceMono">Specifies if the sound should be converted to mono.</param>
        /// <param name="channels">Reference to a variable to store amount of channels</param>
        /// <param name="sampleRate">Reference to a variable to store the sample rate</param>
        public void ChangeForceMono(int soundID, bool forceMono, out int channels, out int sampleRate)
        {
            Info info = _table[soundID];
            info.ForceMono = forceMono;
            channels = info.Channels;

            if (forceMono && channels == 2)
                channels = 1;

            sampleRate = _sampleRate;
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
                ForceMono = forceMono
            };
            sampleRate = _sampleRate;
            info.OpusFile = new OggOpusFile(stream, true);
            info.Channels = info.OpusFile.GetChannelCount();
            channels = info.Channels;

            if (forceMono && channels == 2)
                channels = 1;

            _table[soundID] = info;
        }

        /// <summary>
        /// Jumps to a sample of the sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="sampleOffset">Number of the sample to jump to</param>
        public void SeekToSample(int soundID, int sampleOffset)
        {
            Info info = _table[soundID];
            info.OpusFile.PcmSeek(sampleOffset);
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
            /// stores some information about the stream.
            /// </summary>
            public OggOpusFile OpusFile;

            /// <summary>
            /// The sound stream
            /// </summary>
            public Stream Stream;
        } // cls
    } // cls
}