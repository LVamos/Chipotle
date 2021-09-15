using System;
using System.IO;

using Luky;

namespace Sound
{
    public class SoundFileInfo : DebugSO
    {
        public readonly FilePath FullPath;
        public readonly string Name;
        private readonly string _directory;

        private Random _random;

        private int _variants;

        public SoundFileInfo(string name, FilePath fullPath, int variants = 0)
        {
            Name = name ?? throw new ArgumentException(nameof(name));
            FullPath = fullPath;
            _directory = Path.GetDirectoryName(fullPath);

            Assert(variants >= 0, $"{variants} nesmí být menší než 0.");
            Variants = variants;

            _random = new Random();
        }

        public int Variants
        {
            get => _variants;
            set
            {
                Assert(value >= 0, $"{nameof(value)} musí být větší než 0.");
                _variants = value;
            }
        }

        public FilePath this[int variant] => GetVariant(variant);

        public FilePath GetRandomVariant()
            => GetVariant(_random.Next(1, Variants + 1));

        private FilePath GetVariant(int variant)
        {
            Assert(variant >= 1 && variant <= 1 + Variants, nameof(variant));
            return new FilePath(Path.Combine(_directory, $"{Name} {variant.ToString()}{FullPath.Extension}"));
        }
    }
}