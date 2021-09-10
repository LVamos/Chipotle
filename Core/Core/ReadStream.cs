using System.Collections.Generic;
using System.IO;

namespace Luky
{

    /// <summary>
    /// 
    /// </summary>
    public sealed class ReadStream
    {


        public readonly string Path;
        public readonly Stream Stream; // I had hoped to expose only read methods on this class and pass them through to the stream object. But it looks like I have to expose this stream since libraries I depend on use it, such as OpusFileSharp.
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="path"></param>
        /// <param name="stream"></param>
        public ReadStream(string path, Stream stream)
        {
            Path = path;
            Stream = stream;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ReadStream FromFileSystem(string path)
       => new ReadStream(path, File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        public override bool Equals(object obj) => obj is ReadStream stream && Path == stream.Path && EqualityComparer<Stream>.Default.Equals(Stream, stream.Stream);

        public override int GetHashCode()
        {
            int hashCode = -1050933429;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = hashCode * -1521134295 + EqualityComparer<Stream>.Default.GetHashCode(Stream);
            return hashCode;
        }
    } // cls

}