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
		public static float GetLowPassFrequency(AudioSource source) => _soundManager.GetLowPassFrequency(source);
		public static void SetLowPassFrequency(AudioSource source, float frequency) => _soundManager.SetLowPassFrequency(source, frequency);

		public static void MuteSpeech() => _soundManager.MuteSpeech();
		public static float GetLinearRolloffAttenuation(AudioSource source, float defaultVolume) => _soundManager.GetLinearRolloffAttenuation(source, defaultVolume);
		public static float GetLinearRolloffAttenuation(Vector3 position, float minDistance, float maxDistance, float defaultVolume) => _soundManager.GetLinearRolloffAttenuation(position, minDistance, maxDistance, defaultVolume);

		public static void SlideLowPass(AudioSource source, float duration, int targetFrequency) => _soundManager.SlideLowPass(source, duration, targetFrequency);

		/// <summary>
		/// Calculates low pass filter cut off frequency.
		/// </summary>
		/// <param name="obstacle">Type of an obstacle blocking the sound</param>
		/// <returns>int</returns>
		private static int GetLowPassFrequency(ObstacleType obstacle)
		{
			return obstacle switch
			{
				ObstacleType.Wall => OverWallLowpass,
				ObstacleType.ClosedDoor => OverClosedDoorLowpass,
				ObstacleType.ItemOrCharacter => OverObjectLowpass,
				_ => 22000
			};
		}

		/// <summary>
		/// Calculate volume based on obstacle type.
		/// </summary>
		/// <param name="defaultVolume">Default volume for calculation</param>
		/// <param name="fullVolume">Volume used when nothing blocks the sound</param>
		/// <param name="obstacle">Type of an obstacle blocking the sound</param>
		/// <returns>float</returns>
		public static float GetVolumeByObstacle(ObstacleType obstacle, float defaultVolume, float fullVolume)
		{
			return obstacle switch
			{
				ObstacleType.Wall => GetOverWallVolume(defaultVolume),
				ObstacleType.ClosedDoor => GetOverClosedDoorVolume(defaultVolume),
				ObstacleType.ItemOrCharacter => GetOverObjectVolume(defaultVolume),
				_ => fullVolume
			};
		}

		public static void SlideVolume(AudioSource sound, float duration, float targetVolume, bool stopWhenDone = true) => _soundManager.SlideVolume(sound, duration, targetVolume, stopWhenDone);

		public static void DisableLowpass(AudioSource source) => _soundManager.DisableLowPass(source);

		public static void SetLowPass(AudioSource source, int cutOffFrequency) => _soundManager.SetLowPassFrequency(source, cutOffFrequency);

		public static void SetLowPass(AudioSource source, ObstacleType obstacle) => _soundManager.SetLowPassFrequency(source, GetLowPassFrequency(obstacle));

		public static void SetRoomParameters(Zone zone) => _soundManager.SetRoomParameters(zone);

		public static AudioSource Play(string soundName, Vector3 position, float volume = 1, bool loop = false, bool fadeIn = false, float fadingDuration = .5f, string description = null) => _soundManager.Play(soundName, position, volume, loop, fadeIn, fadingDuration, description);

		public static AudioSource PlayMuffled(string soundName, Vector3 position, float volume = 1, bool loop = false, int cutOffFrequency = 22000, string description = null) => _soundManager.PlayMuffled(soundName, position, volume, loop, cutOffFrequency, description);

		public static AudioSource Play2d(string soundName, float volume = 1, bool loop = false, bool fadeIn = false, float fadingDuration = .5f, string description = null) => _soundManager.Play2d(soundName, volume, loop, fadeIn, fadingDuration, description);

		public static AudioMixerGroup ResonanceGroup => _soundManager.ResonanceGroup;
		public static float MasterVolume { get => _soundManager.MasterVolume; set => _soundManager.MasterVolume = value; }
		public static void SlideMasterVolume(float duration, float targetVolume) => _soundManager.SlideMasterVolume(duration, targetVolume);

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
		public static float GetOverClosedDoorVolume(float defaultVolume) => defaultVolume * .5f;
		public static float GetOverOpenDoorVolume(float defaultVolume) => defaultVolume * .7f;

		/// <summary>
		/// Volume used with sound attenuation.
		/// </summary>
		public static float GetOverObjectVolume(float defaultVolume) => defaultVolume * .9f;

		/// <summary>             /// <summary>
		/// Volume used with sound attenuation.
		/// </summary>
		public static float GetOverWallVolume(float defaultVolume) => defaultVolume * .4f;

		public const float DefaultMasterVolume = 1;

		/// <summary>
		/// Lowpass setting for simulation of sounds obstructed by an object.
		/// </summary>
		public const int OverWallLowpass = 500;
		public const int OverClosedDoorLowpass = 1000;
		public const int OverOpenDoorLowpass = 5000;
		public const int OverObjectLowpass = 2000;

		public static AudioClip GetClip(string name, int? variant = null)
		{
			return !_clips.TryGetValue(name, out ClipInfo info)
				? throw new($"Sound not found: {name}.")
				: variant != null ? info[variant.Value] : info.GetRandomClip();
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

		private static float? _volumeBackup;
		public static void Mute(float duration = .5f) => _soundManager?.Mute();

		public static void Unmute(float duration = .5f) => _soundManager.Unmute();

		public static void ConvertTo2d(AudioSource audioSource, bool muffled) => _soundManager.ConvertTo2d(audioSource, muffled);

		/// <summary>
		/// All sounds used in the game
		/// </summary>
		private static Dictionary<string, ClipInfo> _clips = new(StringComparer.OrdinalIgnoreCase);

	}
}
