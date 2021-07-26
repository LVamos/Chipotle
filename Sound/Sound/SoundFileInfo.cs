using Luky;

using System;
using System.IO;
namespace Sound
{
    public class SoundFileInfo : DebugSO
    {
        public readonly string Name;
        public readonly FilePath FullPath;
        public int Variants
        {
            get => _variants;
            set
            {
                Assert(value >= 0, $"{nameof(value)} musí být větší než 0.");
                _variants = value;
            }
        }

        public SoundFileInfo(string name, FilePath fullPath, int variants = 0)
        {
            Name = name ?? throw ArgumentException(nameof(name));
            FullPath = fullPath;
            _directory = Path.GetDirectoryName(fullPath);


            Assert(variants >= 0, $"{variants} nesmí být menší než 0.");
            Variants = variants;

            _random = new Random();
        }

        private readonly string _directory;
        private FilePath GetVariant(int variant)
        {
            Assert(variant >= 1 && variant <= 1 + Variants, nameof(variant));
            return new FilePath(Path.Combine(_directory, $"{Name} {variant.ToString()}{FullPath.Extension}"));
        }

        private Random _random;
        private int _variants;

        public FilePath GetRandomVariant()
            => GetVariant(_random.Next(1, Variants + 1));


        public FilePath this[int variant] => GetVariant(variant);
    }
}
