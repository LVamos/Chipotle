namespace Luky
{
    /// <summary>
    /// Stores a path to a file or directory.
    /// </summary>
    public struct FilePath
    {
        public readonly string Extension;
        public readonly string Path;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path to a file or directory</param>
        public FilePath(string path)
        {
            Path = path;
            Extension = System.IO.Path.GetExtension(Path);
        }

        /// <summary>
        /// Automatically converts a string to FilePath
        /// </summary>
        /// <param name="s"></param>
        public static implicit operator FilePath(string s)
        => new FilePath(s);

        /// <summary>
        /// Automatically converts a FilePath to string
        /// </summary>
        /// <param name="fp">A filepath to convert</param>
        public static implicit operator string(FilePath fp)
         => fp.Path;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        => Path;
    }
}