
using Game;
using Game.Audio;

using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.Audio
{
	public class SoundPool : MonoBehaviour
	{
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
		private (AudioSource[] sounds, string[] names) GetPlayingSounds()
		{
			AudioSource[] playingSounds = _pool.Concat(_muffledPool).Where(a => a.isPlaying && !a.isVirtual)
.ToArray();
			string[] playingSoundNames = playingSounds.Select(s => s.clip.name).ToArray();
			return (playingSounds, playingSoundNames);
		}

		public AudioLowPassFilter EnableLowPass(AudioSource source)
		{
			_pool.Remove(source);
			source.outputAudioMixerGroup = null;
			source.spatialize = false;
			ResonanceAudioSource resonance = source.GetComponent<ResonanceAudioSource>();
			resonance.enabled = false;
			resonance.bypassRoomEffects = true;
			AudioLowPassFilter lowPass = source.GetComponent<AudioLowPassFilter>();
			lowPass.enabled = true;
			_muffledPool.Add(source);
			return lowPass;
		}

		public AudioSource DisableLowPass(AudioSource source)
		{
			_muffledPool.Remove(source);
			AudioLowPassFilter lowPass = source.GetComponent<AudioLowPassFilter>()
			?? throw new LowPassFilterNotFoundException(source);
			lowPass.enabled = false;
			ResonanceAudioSource resonance = source.GetComponent<ResonanceAudioSource>();
			resonance.enabled = true;
			resonance.bypassRoomEffects = false;
			source.outputAudioMixerGroup = Sounds.MixerGroup;
			source.spatialize = true;
			_pool.Add(source);
			return source;
		}

		public AudioSource GetMuffledSource()
		{
			AudioSource source = _muffledPool.FirstOrDefault(s => !s.isPlaying);
			if (source == null)
				source = AddMuffledSource();
			source.gameObject.SetActive(true);
			return source;
		}

		public AudioSource GetSource()
		{
			AudioSource source = _pool.FirstOrDefault(s => !s.isPlaying)
				?? AddSource();

			source.gameObject.SetActive(true);
			return source;
		}

		private List<AudioSource> _pool = new();
		private List<AudioSource> _muffledPool = new();
		private const int _poolSize = 20;

		private void Start()
		{
			for (int i = 0; i < _poolSize; i++)
				AddSource();
			for (int i = 0; i < _poolSize; i++)
				AddMuffledSource();
		}

		private void Update()
		{
			LogPlayingSounds();

			List<AudioSource> allSources = _pool.Concat(_muffledPool).ToList();
			foreach (AudioSource source in allSources)
			{
				StopForbiddenSound(source);
				if (!source.isPlaying)
					Sleep(source);
			}
		}

		private void StopForbiddenSound(AudioSource source)
		{
			if (!source.isPlaying)
				return;

			if (source.clip.name != Settings.AllowedSound)
				source.Stop();
		}

		private static void Sleep(AudioSource source)
		{
			source.name = "inactive sound";
			source.transform.SetParent(null);
			source.gameObject.SetActive(false);
		}

		private AudioSource AddMuffledSource()
		{
			GameObject o = new("Sound");
			AudioSource source = o.AddComponent<AudioSource>();
			o.AddComponent<ResonanceAudioSource>();
			o.AddComponent<AudioLowPassFilter>();
			o.SetActive(false);
			_muffledPool.Add(source);
			return source;
		}

		private AudioSource AddSource()
		{
			GameObject o = new("Sound");
			AudioSource source = o.AddComponent<AudioSource>();
			o.AddComponent<ResonanceAudioSource>();
			AudioLowPassFilter lowPass = o.AddComponent<AudioLowPassFilter>();
			lowPass.enabled = false;
			o.SetActive(false);
			_pool.Add(source);
			return source;
		}
	}
}
