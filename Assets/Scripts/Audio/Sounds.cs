using Assets.Scripts.Audio;

using Game.Terrain;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
	public static class Sounds
	{
		public static void SlideVolume(AudioSource sound, float duration, float targetVolume)
			=> _soundManager.SlideVolume(sound, duration, targetVolume);

		public static AudioSource DisableLowpass(AudioSource source)
			=> _soundManager.DisableLowPass(source);

		public static void SetLowPass(AudioSource source, int cutOffFrequency)
			=> _soundManager.SetLowPass(source, cutOffFrequency);

		public static void SetRoomParameters(Locality locality) => _soundManager.SetRoomParameters(locality);
		public static AudioSource Play(string soundName, Vector3 position, float volume = 1, bool loop = false, bool fadeIn = false, float fadingDuration = .5f)
			=> _soundManager.Play(soundName, position, volume, loop, fadeIn, fadingDuration);
		public static AudioSource PlayMuffled(string soundName, Vector3 position, float volume = 1, bool loop = false, int cutOffFrequency = 22000) => _soundManager.PlayMuffled(soundName, position, volume, loop, cutOffFrequency);

		public static AudioSource Play2d(string soundName, float volume = 1, bool loop = false, bool fadeIn = false, float fadingDuration = .5f)
			=> _soundManager.Play2d(soundName, volume, loop, fadeIn, fadingDuration);
		public static AudioMixerGroup MixerGroup => _soundManager.MixerGroup;
		public static float MasterVolume { get => _soundManager.MasterVolume; set => _soundManager.MasterVolume = value; }
		public static void AdjustMasterVolume(float duration, float targetVolume) => _soundManager.AdjustMasterVolume(duration, targetVolume);

		public static void StopAll() => _soundManager.StopAllSounds();

		public static void Initialize()
		{
			GameObject obj = new("Sound manager");
			_soundManager = obj.AddComponent<SoundManager>();
		}

		private static SoundManager _soundManager;

		/// <summary>
		/// Volume used with sound attenuation.
		/// </summary>
		public static float GetOverDoorVolume(float defaultVolume)
			=> defaultVolume * .5f;

		/// <summary>
		/// Volume used with sound attenuation.
		/// </summary>
		public static float GetOverObjectVolume(float defaultVolume)
			=> defaultVolume * .9f;

		/// <summary>             /// <summary>
		/// Volume used with sound attenuation.
		/// </summary>
		public static float GetOverWallVolume(float defaultVolume)
			=> defaultVolume * .4f;

		public const float DefaultMasterVolume = 1;

		/// <summary>
		/// Lowpass setting for simulation of sounds obstructed by an object.
		/// </summary>
		public const int OverWallLowpass = 500;
		public const int OverDoorLowpass = 1000;
		public const int OverObjectLowpass = 2000;

		public static AudioClip GetClip(string name, int? variant = null)
		{
			if (!_clips.TryGetValue(name, out ClipInfo info))
				throw new($"Sound not found: {name}.");

			if (variant != null)
				return info[variant.Value];
			return info.GetRandomClip();
		}

		/// <summary>
		/// Loads sound files into memory.
		/// </summary>
		public static void LoadClips()
		{
			List<string> files = Directory.EnumerateFiles(MainScript.SoundPath, "*.*", SearchOption.AllDirectories)
				.Where(f => new[] { ".mp3", ".wav", ".flac", ".ogg" }
					.Contains(Path.GetExtension(f).ToLower()))
				.ToList();
			HashSet<string> usedNames = new();

			foreach (string path in files)
			{
				string fileName = Path.GetFileNameWithoutExtension(path);
				if (!usedNames.Add(fileName))
					throw new($"Duplicate file name: {path}.");

				string[] nameParts = fileName.Split(' ');
				if (nameParts.Length < 2 || !int.TryParse(nameParts[1], out int variant) || variant < 0)
					throw new($"Invalid file name: {path}.");

				string shortName = nameParts[0].ToLower();

				if (!_clips.TryGetValue(shortName, out ClipInfo info))
				{
					info = new ClipInfo(shortName);
					_clips[shortName] = info;

					string[] sounds = files
					.Where(f => Path.GetFileNameWithoutExtension(f).ToLower().StartsWith(shortName + " "))
						.ToArray();
					info.LoadClips(sounds);
				}
			}
		}

		public static void MasterFadeOut(float duration) => AdjustMasterVolume(duration, 0);

		/// <summary>
		/// All sounds used in the game
		/// </summary>
		private static Dictionary<string, ClipInfo> _clips = new(StringComparer.OrdinalIgnoreCase);


	}
}
