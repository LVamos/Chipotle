using OpusfileSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class OpusFileDecoder : DebugSO, IDecoder
    { // uses OpusFileSharp which is a PInvoke wrapper around libopusfile
        private const int _sampleRate = 48000;

        // I had to compile it for release and .NET 4 client profile, by default it was set to .NET 4.5.
        // this maps soundIDs to their associated info.
        private Dictionary<int, Info> mTable = new Dictionary<int, Info>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="shortsRead"></param>
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
            } // end for loop
            return shortsRead / 2;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            // dispose each sound
            foreach (int soundID in mTable.Keys.ToArray())
                DisposeStream(soundID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        public void DisposeStream(int soundID)
        {
            Info info = mTable[soundID];
            mTable.Remove(soundID);
            info.OpusFile.Dispose();
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
            Info info = mTable[soundID];
            int samplesRead = info.OpusFile.Read(buffer, 0, buffer.Length);
            if (samplesRead == 0)
            { // if we are not looping, we are done, otherwise we seek to the beginning of the stream.
                if (!info.Looping)
                    return 0; // nothing to read.
                else // we seek to the beginning of the stream and try reading again.
                { // I tried just seeking without disposing the OggOpusFile object, but it didn't work.
                    info.OpusFile.Dispose();
                    info.Stream.Seek(0, SeekOrigin.Begin);
                    info.OpusFile = new OggOpusFile(info.Stream, true);
                    samplesRead = info.OpusFile.Read(buffer, 0, buffer.Length);
                }
            }
            // I'm used to knowing how many shorts were read, rather than how many samples.
            int shortsRead = samplesRead * info.Channels;
            if (info.ForceMono && info.Channels == 2)
                shortsRead = ConvertStereoToMono(buffer, shortsRead);

            return shortsRead;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <returns></returns>
        public short[] GetBuffer(int soundID)
        {
            Info info = mTable[soundID];
            // a read call usually returns 1920 shorts, so we match that size with our buffer by default.
            short[] buffer = new short[1920];
            int shortsRead = FillBuffer(soundID, buffer);
            if (shortsRead == buffer.Length)
                return buffer;
            // otherwise we need to copy to a new buffer of the right length, so all of it is usable.
            short[] buffer2 = new short[shortsRead];
            System.Buffer.BlockCopy(buffer, 0, buffer2, 0, shortsRead * 2);
            return buffer2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="current"></param>
        public void GetDynamicInfo(int soundID, out int current)
        {
            Info info = mTable[soundID];
            current = (int)info.OpusFile.PcmTell;
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
            Info info = mTable[soundID];
            sampleRate = _sampleRate;
            totalSamples = (int)info.OpusFile.GetPcmTotal();
            channels = info.Channels;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="forceMono"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        public void ChangeForceMono(int soundID, bool forceMono, out int channels, out int sampleRate)
        {
            Info info = mTable[soundID];
            info.ForceMono = forceMono;
            channels = info.Channels;
            if (forceMono && channels == 2)
                channels = 1;

            sampleRate = _sampleRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="looping"></param>
        public void ChangeLooping(int soundID, bool looping)
        {
            Info info = mTable[soundID];
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
            Info info = new Info();
            info.Stream = stream;
            info.Looping = looping;
            info.ForceMono = forceMono;
            sampleRate = _sampleRate;
            info.OpusFile = new OggOpusFile(stream, true);
            info.Channels = info.OpusFile.GetChannelCount();
            // note that we leave info.Channels as the number of channels we are reading, but we may change the channels variable we return for playback if we are forcing mono.
            channels = info.Channels;
            //Say("{0} {1}", forceMono, channels);
            if (forceMono && channels == 2)
                channels = 1;

            mTable[soundID] = info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="soundID"></param>
        /// <param name="sampleOffset"></param>
        public void SeekToSample(int soundID, int sampleOffset)
        {
            Info info = mTable[soundID];
            //ian here's where I need to see if the stream is an AssetStream, and if so, check the CanSeek bool and convert it to a seekable stream if necessary.
            // actually I decided to just prime the last 64K of every file, so they can always start out with seeking support and a known file duration, but leaving this comment in case I decide to change that later.
            info.OpusFile.PcmSeek(sampleOffset);
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class Info
        {
            public bool ForceMono;
            public int Channels;
            public bool Looping;
            public OggOpusFile OpusFile;
            public Stream Stream;
        } // cls
    } // cls
}