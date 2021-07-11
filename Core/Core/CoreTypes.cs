using System.IO;

namespace Luky
{
	public sealed class FileToken : DebugSO
	{
		public readonly string FilePath;
		public readonly string ShortPath;
		public readonly Name Name;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="shortPath"></param>
		public FileToken(string filePath, string shortPath)
		{
			FilePath = filePath;
			ShortPath = shortPath;
			Name = Path.GetFileNameWithoutExtension(filePath);
		}
	} // cls
}
