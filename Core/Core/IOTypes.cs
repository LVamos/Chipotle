using System.IO;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    public struct FilePath
    {
        // just to make things more type safe I'd rather pass around a type of FilePath than a type of string.
        public readonly string Path;
        public readonly string Extension;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="path"></param>
        public FilePath(string path)
        {
            Path = path;
            Extension = System.IO.Path.GetExtension(Path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public static implicit operator FilePath(string s)
        => new FilePath(s);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fp"></param>
        public static implicit operator string(FilePath fp)
         => fp.Path;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        => Path;
    }

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
    } // cls

}