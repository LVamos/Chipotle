using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NLibsndfile.Native;

namespace Luky
{
    /// <summary>
    /// LSF stands for libsndfile
    /// </summary>
    internal sealed class LSFDecoder : DebugSO, IDecoder
    {
        private const int _bufferSize  = 24000; // at 48000Hz that is 500ms mono, or 250ms stereo.

        private readonly short[] _tempBuffer  = new short[_bufferSize ];

        private readonly byte[] _vioBuffer = new byte[_bufferSize  * 2];
        private LibsndfileApi _api = new LibsndfileApi();

        // this maps soundIDs to their associated info.
        private Dictionary<int, Info> _table = new Dictionary<int, Info>();

        private SF_VIRTUAL_IO _virtualIO;

        /// <summary>
        /// constructor
        /// </summary>
        public LSFDecoder()
        {
            var vio = new SF_VIRTUAL_IO();
            vio.get_filelen = get_filelen;
            vio.seek = seek;
            vio.read = read;
            vio.write = write;
            vio.tell = tell;
            _virtualIO = vio;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        { // dispose each sound
            foreach (int soundID in _table.Keys.ToArray())
                DisposeStream(soundID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        public void DisposeStream(int soundID)
        {
            var info = _table[soundID];
            _table.Remove(soundID);
            _api.Close(info.SFHandle);
            info.Stream.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int FillBuffer(int soundID, short[] buffer)
        { 
            // returns the number of shorts written to the buffer
            var info = _table[soundID];

            long totalRead = 0;
            long totalWrote = 0; // when doing stereo to mono conversion the totalWrote is not the same as the totalRead
            long lengthToRead = buffer.Length;
            if (info.ForceMono && info.LSFInfo.Channels == 2)
                lengthToRead *= 2; // read twice as much
            while (totalRead != lengthToRead)
            {
                if (info.Looping && info.Position == info.TotalLength)
                { // we reached the end of the stream but we're looping so we need to start back at the beginning
                    info.Position = 0;
                    _api.Seek(info.SFHandle, 0, (int)SeekOrigin.Begin);
                }
                if (!info.Looping && info.Position == info.TotalLength)
                { // we reached the end of the stream but we aren't looping, so we're done.
                    break;
                }
                long lesser = Math.Min(_tempBuffer .Length, info.TotalLength - info.Position);
                lesser = Math.Min(lesser, lengthToRead - totalRead);
                long read = _api.ReadItems(info.SFHandle, _tempBuffer , lesser);
                if (info.ForceMono && info.LSFInfo.Channels == 2)
                    ConvertToMono(_tempBuffer , buffer, read, ref totalWrote);
                else
                {
                    Array.Copy(_tempBuffer , 0, buffer, totalWrote, read);
                    totalWrote += read;
                }
                info.Position += read;
                totalRead += read;
            } // end while not finished reading
            return (int)totalWrote;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="currentSample"></param>
        public void GetDynamicInfo(int soundID, out int currentSample)
        {
            var info = _table[soundID];
            currentSample = (int)info.Position / info.LSFInfo.Channels;
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
            var info = _table[soundID];
            sampleRate = info.LSFInfo.SampleRate;
            totalSamples = (int)info.LSFInfo.Frames;
            channels = info.Channels;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="looping"></param>
        public void ChangeLooping(int soundID, bool looping)
        {
            var info = _table[soundID];
            info.Looping = looping;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="stream"></param>
        /// <param name="looping"></param>
        /// <param name="forceMono"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        public void InitStream(int soundID, Stream stream, bool looping, bool forceMono, out int channels, out int sampleRate)
        {
            var info = new Info();
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
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="seekSample"></param>
        public void SeekToSample(int soundID, int seekSample)
        {
            var info = _table[soundID];
            _api.Seek(info.SFHandle, seekSample, (int)SeekOrigin.Begin);
            info.Position = seekSample * info.Channels;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="read"></param>
        /// <param name="totalWrote"></param>
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
        /// 
        /// </summary>
        /// <param name="user_data"></param>
        /// <returns></returns>
        private Int64 get_filelen(int user_data)
        => GetStream(user_data).Length; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <returns></returns>
        private Stream GetStream(int soundID)
        => _table[soundID].Stream; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <param name="count"></param>
        /// <param name="user_data"></param>
        /// <returns></returns>
        private Int64 read(IntPtr pBuffer, Int64 count, int user_data)
        {
            //Say(count); // so I can learn the buffer size that libsndfile actually uses, though maybe it is based on what I ask it for when calling ReadItems.
            int lesser = Math.Min(_vioBuffer.Length, (int)count);
            int actual = GetStream(user_data).Read(_vioBuffer, 0, lesser);
            // copy it to the pointer.
            Marshal.Copy(_vioBuffer, 0, pBuffer, actual);
            return actual;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="whence"></param>
        /// <param name="user_data"></param>
        /// <returns></returns>
        private Int64 seek(Int64 offset, SeekOrigin whence, int user_data)
        => GetStream(user_data).Seek(offset, whence); 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_data"></param>
        /// <returns></returns>
        private Int64 tell(int user_data)
        => GetStream(user_data).Position; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <param name="count"></param>
        /// <param name="user_data"></param>
        /// <returns></returns>
        private Int64 write(IntPtr pBuffer, Int64 count, int user_data)
        {
            // we have to bring the bytes into a managed byte array.
            var buffer = new byte[count];
            Marshal.Copy(pBuffer, buffer, 0, (int)count);
            GetStream(user_data).Write(buffer, 0, buffer.Length);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class Info
        {
            public bool ForceMono;
            public int Channels;
            public bool Looping;
            public LibsndfileInfo LSFInfo = new LibsndfileInfo();
            public long Position = 0;
            public IntPtr SFHandle;
            public Stream Stream;
            public long TotalLength;
        } // cls
    } // cls
}