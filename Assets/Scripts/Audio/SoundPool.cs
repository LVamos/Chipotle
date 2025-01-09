
using System.Collections.Generic;
using System.Linq;

using Unity.VisualScripting;

using UnityEngine;

namespace Assets.Scripts.Audio
{
	public class SoundPool : MonoBehaviour
	{
		public AudioLowPassFilter EnableLowPass(AudioSource source)
		{
			_pool.Remove(source);
			AudioLowPassFilter lowPass = source.AddComponent<AudioLowPassFilter>();
			_muffledPool.Add(source);
			return lowPass;
		}

		public AudioSource DisableLowPass(AudioSource source)
		{
			_muffledPool.Remove(source);
			AudioLowPassFilter lowPass = source.GetComponent<AudioLowPassFilter>()
				?? throw new LowPassFilterNotFoundException(source);
			Destroy(lowPass);
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
			string[] names = _pool.Concat(_muffledPool).Where(a => a.isPlaying).Select(s => s.clip.name).ToArray();
			AudioSource[] a = _pool.Concat(_muffledPool).Where(a => a.isPlaying).ToArray();

			foreach (AudioSource source in _pool.Concat(_muffledPool))
			{
				if (!source.isPlaying)
					source.gameObject.SetActive(false);
			}
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
			o.SetActive(false);
			_pool.Add(source);
			return source;
		}
	}
}
