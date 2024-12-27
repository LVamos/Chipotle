using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace Game.Audio
{
	/// <summary>
	/// Provides information about a sound file.
	/// </summary>
	public class ClipInfo
	{
		private int GenerateRandomNumber(int min, int max, int ignore)
		{
			while (true)
			{
				int result = _random.Next(min, max + 1); // max is exclusive, hence +1
				if (result != ignore)
					return result;
			}
		}

		/// <summary>
		/// Index of the last played variant of the sound.
		/// </summary>
		private int _lastVariant;

		public AudioClip GetRandomClip()
		{
			int variant = Variants == 1 ? 0 : GenerateRandomNumber(0, Variants - 1, _lastVariant);
			_lastVariant = variant;
			return _clips[variant];
		}

		private string ProcessPath(string filePath)
		{
			// Removes "Assets\\Resources\\" (case-insensitive) z cesty
			string resourcesPath = "Assets\\Resources\\";
			string processedPath = filePath;

			if (processedPath.StartsWith(resourcesPath, StringComparison.OrdinalIgnoreCase))
				processedPath = processedPath.Substring(resourcesPath.Length);

			// Remvoes extension.
			processedPath = Path.ChangeExtension(processedPath, null);

			return processedPath;
		}

		/// <summary>
		/// Loads the sounds into memory.
		/// </summary>
		/// <param name="pathes">Pathes to the sound files</param>
		public void LoadClips(string[] pathes)
		{
			// Load the files into memory.
			foreach (string path in pathes)
			{
				string finalPath = ProcessPath(path);
				AudioClip clip = Resources.Load<AudioClip>(finalPath)
					?? throw new InvalidOperationException("Unable to load the clip");
				if (clip.loadState != AudioDataLoadState.Loaded)
					clip.LoadAudioData();
				_clips.Add(clip);
			}
		}

		/// <summary>
		/// Name of the file without extension
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// Random numbers generator
		/// </summary>
		private readonly System.Random _random = new();

		/// <summary>
		/// Variants of one sound loaded into memory.
		/// </summary>
		private List<AudioClip> _clips = new List<AudioClip>();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of the sound</param>
		/// <param name="fullPath">Path to the sound file</param>
		/// <param name="variants">Number of variants for the sound</param>
		public ClipInfo(string name)
			=> Name = name ?? throw new ArgumentException(nameof(name));

		/// <summary>
		/// Number of variants for the sound
		/// </summary>
		public int Variants => _clips.Count;


		/// <summary>
		/// Provides access to all variants of the sound.
		/// </summary>
		/// <param name="variant">Number of required sound variant</param>
		/// <returns>Full path to the required sound variant</returns>
		public AudioClip this[int variant] => GetVariant(variant);

		/// <summary>
		/// Returns full path to the required sound variant.
		/// </summary>
		/// <param name="variant">Number of the variant</param>
		/// <returns>Full path to the sound variant</returns>
		private AudioClip GetVariant(int variant)
		{
			if (variant >= _clips.Count)
				throw new ArgumentException(nameof(variant));

			return _clips[variant];
		}
	}
}