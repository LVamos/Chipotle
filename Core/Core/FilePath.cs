namespace Luky
{
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

}