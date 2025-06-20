
using Game;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEditor;

using UnityEngine;

namespace Assets.Scripts.Audio
{
	public class SoundPool : MonoBehaviour
	{
		public void SoundStartedPlaying(AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			_pool.Remove(source);
			_playingSources.Add(source);
		}

		public AudioLowPassFilter GetLowPass(AudioSource source) => _lowPasses[source];

		private Dictionary<AudioSource, AudioLowPassFilter> _lowPasses = new();

		private void LogPlayingSounds()
		{
			if (!Settings.LogPlayingSounds)
				return;

			(AudioSource[] sounds, string[] names) sounds = GetPlayingSounds();
			string[] soundDescriptions = sounds.sounds.Select(s => s.name).ToArray();
			StringBuilder builder = new("Playing sounds");
			builder.AppendLine();
			for (int i = 0; i < sounds.names.Length; i++)
			{
				builder.AppendLine($"{sounds.names[i]}; {soundDescriptions[i]}");
			}

			string output = builder.ToString();
			System.Diagnostics.Debug.WriteLine(output);
		}

		/// <summary>
		/// Returns currently playing sounds.
		/// </summary>
		/// <returns>(AudioSource[] sounds, string[] names)</returns>
		public (AudioSource[] sounds, string[] names) GetPlayingSounds()
		{
			AudioSource[] playingSounds = _playingSources.ToArray();
			string[] playingSoundNames = playingSounds.Select(s => s.clip.name).ToArray();
			return (playingSounds, playingSoundNames);
		}

		public void EnableLowPass(AudioSource source)
		{
			AudioLowPassFilter lowPass = GetLowPass(source);
			lowPass.enabled = true;
			source.spatializePostEffects = true;
		}

		public void DisableLowPass(AudioSource source)
		{
			AudioLowPassFilter lowPass = GetLowPass(source);
			if (!lowPass.enabled)
				return;

			source.spatializePostEffects = false;
			lowPass.enabled = false;
		}

		private HashSet<AudioSource> _playingSources = new(_poolSize);
		public AudioSource GetSource()
		{
			AudioSource source = _pool.FirstOrDefault();
			if (source == null)
				source = AddSource();

			source.gameObject.SetActive(true);
			_playingSources.Add(source);
			return source;
		}

		private HashSet<AudioSource> _pool = new(_poolSize);
		private const int _poolSize = 20;

		private void Start()
		{
			for (int i = 0; i < _poolSize; i++)
				AddSource();
		}

		private void Update()
		{
			LogPlayingSounds();
			HashSet<AudioSource> sourcesToRemove = new();
			foreach (AudioSource source in _playingSources)
			{
				StopForbiddenSound(source);
				if (!source.isPlaying)
				{
					Sleep(source);
					sourcesToRemove.Add(source);
					_pool.Add(source);
				}
			}

			foreach (AudioSource source in sourcesToRemove)
				_playingSources.Remove(source);
		}

		private void StopForbiddenSound(AudioSource source)
		{
			if (!source.isPlaying)
				return;

			if (!string.IsNullOrEmpty(Settings.AllowedSound) && source.clip.name != Settings.AllowedSound)
				source.Stop();
		}

		private static void Sleep(AudioSource source)
		{
			source.name = "inactive sound";
			source.transform.SetParent(null);
			source.gameObject.SetActive(false);
		}


		private AudioSource AddSource()
		{
			GameObject o = new("Sound");
			AudioSource source = o.AddComponent<AudioSource>();
			o.AddComponent<ResonanceAudioSource>();
			AudioLowPassFilter lowPass = o.AddComponent<AudioLowPassFilter>();
			o.SetActive(false);
			_pool.Add(source);
			_lowPasses[source] = lowPass;
			return source;
		}
	}
}
