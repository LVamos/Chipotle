using System.IO;

namespace Luky
{

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
    }

}