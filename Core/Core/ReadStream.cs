using System.IO;

namespace Luky
{
    /// <summary>
    /// Encapsulates a read stream
    /// </summary>
    public sealed class ReadStream
    {
        /// <summary>
        /// Path to a file that is streamed
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// The stream
        /// </summary>
        public readonly Stream Stream;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to the source</param>
        /// <param name="stream">Instance of a stream</param>
        public ReadStream(string path, Stream stream)
        {
            Path = path;
            Stream = stream;
        }

        /// <summary>
        /// Opens a file as a stream.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>Instance of ReadStream</returns>
        public static ReadStream FromFileSystem(string path)
       => new ReadStream(path, File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
    }
}