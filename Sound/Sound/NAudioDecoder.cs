using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class NAudioDecoder : DebugSO, IDecoder
    {
        private const int _bytesPerSample  = 2;

        private byte[] mBuffer = new byte[3840];

        /// <summary>
        /// this maps soundIDs to their associated info.
        /// </summary>
        private Dictionary<int, Info> _table = new Dictionary<int, Info>();

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
            info.Reader.Dispose();
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
            var info = _table[soundID];
            int bytesRead = info.Reader.Read(mBuffer, 0, mBuffer.Length);
            if (bytesRead == 0)
            { // if we are not looping, we are done, otherwise we seek to the beginning of the stream.
                if (!info.Looping)
                    return 0; // nothing to read.
                else // we seek to the beginning of the stream and try reading again.
                {
                    info.Reader.Dispose();
                    info.Stream.Seek(0, SeekOrigin.Begin);
                    info.Reader = new StreamMediaFoundationReader(info.Stream);
                    bytesRead = info.Reader.Read(mBuffer, 0, mBuffer.Length);
                }
            }
            int shortsRead = bytesRead / 2;
            if (buffer.Length < shortsRead)
                throw Exception("buffer is too small, has length {0} but needs length {1}", buffer.Length, shortsRead);
            System.Buffer.BlockCopy(mBuffer, 0, buffer, 0, bytesRead);
            if (info.ForceMono && info.Channels == 2)
                shortsRead = OpusFileDecoder.ConvertStereoToMono(buffer, shortsRead);
            return shortsRead;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="currentSample"></param>
        public void GetDynamicInfo(int soundID, out int currentSample)
        {
            var info = _table[soundID];
            currentSample = (int)info.Reader.Position / _bytesPerSample  / info.Channels;
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
            // 3840 is 1920 * 2
            var info = _table[soundID];
            sampleRate = info.Reader.WaveFormat.SampleRate;
            totalSamples = (int)info.Reader.Length / _bytesPerSample  / info.Channels;
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
            info.Stream = stream;
            info.Looping = looping;
            info.ForceMono = forceMono;
            info.Reader = new StreamMediaFoundationReader(stream);
            sampleRate = info.Reader.WaveFormat.SampleRate;
            info.Channels = info.Reader.WaveFormat.Channels;
            // note that we leave info.Channels as the number of channels we are reading, but we may change the channels variable we return for playback if we are forcing mono.
            channels = info.Channels;
            if (forceMono && channels == 2)
                channels = 1;
            _table[soundID] = info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="seekSample"></param>
        public void SeekToSample(int soundID, int seekSample)
        {
            var info = _table[soundID];
            info.Reader.Position = seekSample * _bytesPerSample  * info.Channels;
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class Info
        {
            public bool ForceMono;
            public int Channels;
            public bool Looping;

            // I was getting errors about sample rate changes on some mp3 files.
            // Mark Heath, the author of NAudio recommended using MediaFoundationReader instead of Mp3FileReader.
            // see this page: https://github.com/naudio/NAudio/issues/229
            //public Mp3FileReader Reader;
            public StreamMediaFoundationReader Reader;

            public Stream Stream;
        } // cls
    } // cls
}