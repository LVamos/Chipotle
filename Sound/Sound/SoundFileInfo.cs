using Luky;

using System;
using System.IO;

namespace Sound
{
    /// <summary>
    /// Provides information about a sound file.
    /// </summary>
    public class SoundFileInfo : DebugSO
    {
        /// <summary>
        /// Path to the file
        /// </summary>
        public readonly FilePath FullPath;

        /// <summary>
        /// Name of the file without extension
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Path to the directory the file is placed in
        /// </summary>
        private readonly string _directory;

        /// <summary>
        /// Random numbers generator
        /// </summary>
        private readonly Random _random;

        /// <summary>
        /// Number of variants of the sound
        /// </summary>
        private int _variants;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the sound</param>
        /// <param name="fullPath">Path to the sound file</param>
        /// <param name="variants">Number of variants for the sound</param>
        public SoundFileInfo(string name, FilePath fullPath, int variants = 0)
        {
            Name = name ?? throw new ArgumentException(nameof(name));
            FullPath = fullPath;
            _directory = Path.GetDirectoryName(fullPath);

            Assert(variants >= 0, $"{variants} nesmí být menší než 0.");
            Variants = variants;

            _random = new Random();
        }

        /// <summary>
        /// Number of variants for the sound
        /// </summary>
        public int Variants
        {
            get => _variants;
            set
            {
                Assert(value >= 0, $"{nameof(value)} musí být větší než 0.");
                _variants = value;
            }
        }

        /// <summary>
        /// Provides access to all variants of the sound.
        /// </summary>
        /// <param name="variant">Number of required sound variant</param>
        /// <returns>Full path to the required sound variant</returns>
        public FilePath this[int variant] => GetVariant(variant);

        /// <summary>
        /// Returns full path to a random variant of this sound.
        /// </summary>
        /// <returns></returns>
        public FilePath GetRandomVariant()
            => GetVariant(_random.Next(1, Variants + 1));

        /// <summary>
        /// Returns full path to the required sound variant.
        /// </summary>
        /// <param name="variant">Number fo the variant</param>
        /// <returns>Full path to the sound variant</returns>
        private FilePath GetVariant(int variant)
        {
            Assert(variant >= 1 && variant <= 1 + Variants, nameof(variant));
            return new FilePath(Path.Combine(_directory, $"{Name} {variant}{FullPath.Extension}"));
        }
    }
}