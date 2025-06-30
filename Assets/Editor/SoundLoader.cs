using Assets.Scripts;

using Game;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEditor;

namespace Assets.Editor
{
	[InitializeOnLoad]
	public static class SoundLoader
	{
		private static HashSet<string> _allowedExtensions = new()
		{
			".mp3",
			".wav",
			".flac",
			".ogg"
		};

		public static void LoadSoundFiles()
		{
			string soundDirectoryPath = "Assets/Resources/" + MainScript.SoundPath;
			List<string> soundNames = FindSoundFiles(soundDirectoryPath);
			string soundListPath = soundDirectoryPath + "/soundlist.txt";
			string content = string.Join("\n", soundNames);
			content = content.Replace("\\", "/");
			File.WriteAllText(soundListPath, content);
			TolkTolk.Speak("Seznam zvuků vytvořen");
		}

		private static List<string> FindSoundFiles(string path)
		{
			List<string> files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
				.Where(IsAllowedExtension)
				.Select(path => path.Substring("Assets/Resources/".Length))
				.Select(GetPathWithoutExtension)
				.ToList();
			return files;
		}

		private static string GetPathWithoutExtension(string path)
		{
			return Path.Combine(
				Path.GetDirectoryName(path),
				Path.GetFileNameWithoutExtension(path));
		}

		private static bool IsAllowedExtension(string f)
		{
			return _allowedExtensions.Contains(Path.GetExtension(f).ToLower());
		}
	}
}
