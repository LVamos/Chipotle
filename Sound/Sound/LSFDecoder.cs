using NLibsndfile.Native;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Luky
{
    /// <summary>
    /// Decodes sound files using the libsndfile library.
    /// </summary>
    internal sealed class LSFDecoder : DebugSO, IDecoder
    {
        /// <summary>
        /// Size of the buffer used for sound file streaming
        /// </summary>
        private const int _bufferSize = 24000;

        /// <summary>
        /// The libsndfile library interface
        /// </summary>
        private readonly LibsndfileApi _api = new LibsndfileApi();

        /// <summary>
        /// Maps sound IDs to their associated info.
        /// </summary>
        private readonly Dictionary<int, Info> _table = new Dictionary<int, Info>();

        /// <summary>
        /// A buffer used for sound file reading and writing to sound streams
        /// </summary>
        private readonly short[] _tempBuffer = new short[_bufferSize];

        /// <summary>
        /// A buffer used for sound file reading and writing to sound streams
        /// </summary>
        private readonly byte[] _vioBuffer = new byte[_bufferSize * 2];

        /// <summary>
        /// Manipulates with a sound stream.
        /// </summary>
        private SF_VIRTUAL_IO _virtualIO;

        /// <summary>
        /// constructor
        /// </summary>
        public LSFDecoder()
        {
            SF_VIRTUAL_IO vio = new SF_VIRTUAL_IO
            {
                get_filelen = GetFileLength,
                seek = Seek,
                read = Read,
                write = Write,
                tell = GetPosition
            };
            _virtualIO = vio;
        }

        /// <summary>
        /// Disposes the decoder.
        /// </summary>
        public void Dispose()
        { // dispose each sound
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
            _api.Close(info.SFHandle);
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
            long totalRead = 0;
            long totalWrote = 0; // when doing stereo to mono conversion the totalWrote is not the same as the totalRead.
            long lengthToRead = buffer.Length;

            if (info.ForceMono && info.LSFInfo.Channels == 2)
                lengthToRead *= 2; // Read twice more.

            while (totalRead != lengthToRead)
            {
                if (info.Looping && info.Position == info.TotalLength)
                {
                    // It reached the end of the stream but looping is enabled so it must start back
                    // at the beginning.
                    info.Position = 0;
                    _api.Seek(info.SFHandle, 0, (int)SeekOrigin.Begin);
                }

                if (!info.Looping && info.Position == info.TotalLength) // It reached the end of the stream but looping is disabled, so it can be stopped.
                    break;

                long lesser = Math.Min(_tempBuffer.Length, info.TotalLength - info.Position);
                lesser = Math.Min(lesser, lengthToRead - totalRead);
                long read = _api.ReadItems(info.SFHandle, _tempBuffer, lesser);

                if (info.ForceMono && info.LSFInfo.Channels == 2)
                    ConvertToMono(_tempBuffer, buffer, read, ref totalWrote);
                else
                {
                    Array.Copy(_tempBuffer, 0, buffer, totalWrote, read);
                    totalWrote += read;
                }

                info.Position += read;
                totalRead += read;
            }

            return (int)totalWrote;
        }

        /// <summary>
        /// Optains current position of the specified sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="currentSample"></param>
        public void GetDynamicInfo(int soundID, out int currentSample)
        {
            Info info = _table[soundID];
            currentSample = (int)info.Position / info.LSFInfo.Channels;
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
            sampleRate = info.LSFInfo.SampleRate;
            totalSamples = (int)info.LSFInfo.Frames;
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
            Info info = new Info();
            _table.Add(soundID, info);
            info.Stream = stream;
            info.Looping = looping;
            info.ForceMono = forceMono;

            info.SFHandle = _api.OpenVirtual(ref _virtualIO, LibsndfileMode.Read, ref info.LSFInfo, soundID);
            if (info.SFHandle == IntPtr.Zero)
            {
                string error = _api.ErrorString(IntPtr.Zero);
                throw new Exception(error);
            }
            channels = info.LSFInfo.Channels;

            if (channels == 2 && forceMono)
                channels = 1;

            info.Channels = channels;
            sampleRate = info.LSFInfo.SampleRate;
            info.TotalLength = info.LSFInfo.Frames * info.LSFInfo.Channels; // 1 short per channel per frame because it's 16 bits per sample.
        }

        /// <summary>
        /// Jumps to a sample of the sound.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <param name="seekSample">Number of the sample to jump to</param>
        public void SeekToSample(int soundID, int seekSample)
        {
            Info info = _table[soundID];
            _api.Seek(info.SFHandle, seekSample, (int)SeekOrigin.Begin);
            info.Position = seekSample * info.Channels;
        }

        /// <summary>
        /// Converts a sample to mono.
        /// </summary>
        /// <param name="input">The sample to be converted</param>
        /// <param name="output">A buffer to store the result of the conversion</param>
        /// <param name="read">Specifies amount of data to be read from the buffer</param>
        /// <param name="totalWrote">Amount of data written to the output</param>
        private void ConvertToMono(short[] input, short[] output, long read, ref long totalWrote)
        {
            if (read % 2 != 0)
                throw new Exception("read an odd number of bytes");

            for (int i = 0; i < read; i += 2)
            {
                // use L/2 + R/2 to combine the stereo channels into mono.
                int combined = (input[i] / 2) + (input[i + 1] / 2);
                output[totalWrote] = (short)combined;
                totalWrote++;
            } // end for loop
        }

        /// <summary>
        /// Returns length of the specified stream in bytes.
        /// </summary>
        /// <param name="soundID">ID of the source sound stream</param>
        /// <returns>Total count of bytes</returns>
        private Int64 GetFileLength(int soundID)
        => GetStream(soundID).Length;

        /// <summary>
        /// Returns current position of the specified sound stream.
        /// </summary>
        /// <param name="soundID">ID of the sound stream</param>
        /// <returns></returns>
        private Int64 GetPosition(int soundID)
        => GetStream(soundID).Position;

        /// <summary>
        /// Returns a stream assigned to the <paramref name="soundID"/>.
        /// </summary>
        /// <param name="soundID">ID of the sound</param>
        /// <returns>The stream</returns>
        private Stream GetStream(int soundID)
        => _table[soundID].Stream;

        /// <summary>
        /// Copies a chunk of data from the specified sound stream to memory.
        /// </summary>
        /// <param name="pBuffer">Unmanaged pointer to the target buffer</param>
        /// <param name="count">Count of samples to be read</param>
        /// <param name="soundID">ID of the source sound</param>
        /// <returns>Count of read bytes</returns>
        private Int64 Read(IntPtr pBuffer, Int64 count, int soundID)
        {
            int lesser = Math.Min(_vioBuffer.Length, (int)count);
            int actual = GetStream(soundID).Read(_vioBuffer, 0, lesser);

            Marshal.Copy(_vioBuffer, 0, pBuffer, actual); // copy it to the pointer.
            return actual;
        }

        /// <summary>
        /// Jumps to the specified position of the stream.
        /// </summary>
        /// <param name="offset">The posisiton in the sound stream</param>
        /// <param name="whence">
        /// Specifies if the position is counted from the beginning or the end of the stream.
        /// </param>
        /// <param name="soundID">ID of the sound</param>
        /// <returns>New position in the stream</returns>
        private Int64 Seek(Int64 offset, SeekOrigin whence, int soundID)
        => GetStream(soundID).Seek(offset, whence);

        /// <summary>
        /// Writes data from memory to the specified stream.
        /// </summary>
        /// <param name="pBuffer">An unmanaged pointer to a buffer</param>
        /// <param name="count">Count of bytes to be written</param>
        /// <param name="soundID"></param>
        /// <returns>The count of written data</returns>
        private Int64 Write(IntPtr pBuffer, Int64 count, int soundID)
        {
            byte[] buffer = new byte[count];
            Marshal.Copy(pBuffer, buffer, 0, (int)count);
            GetStream(soundID).Write(buffer, 0, buffer.Length);
            return count;
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
            public LibsndfileInfo LSFInfo = new LibsndfileInfo();

            /// <summary>
            /// Current position in the stream
            /// </summary>
            public long Position = 0;

            /// <summary>
            /// Handle used for communication with the libsndfile library
            /// </summary>
            public IntPtr SFHandle;

            /// <summary>
            /// The sound stream
            /// </summary>
            public Stream Stream;

            /// <summary>
            /// Length of the sound stream
            /// </summary>
            public long TotalLength;
        }
    }
}