using Game;
using Game.Audio;

using UnityEngine;

namespace Assets.Scripts
{
	public class CutScenePlayer : MonoBehaviour
	{
		private void Update()
		{
			if (!_audio.isPlaying && _audio.time > 0)
				_audio.clip = null;
		}

		private void Awake()
		{
			_audio = gameObject.AddComponent<AudioSource>();
		}

		public float Volume => _audio.volume; public float Position => _audio.time;

		public void Revind(int seconds)
		{
			if (_audio == null)
				return;

			if (Paused || _audio.isPlaying)
				if (_audio.time > seconds)
					_audio.time -= seconds;
		}

		public bool Finished => _audio != null && _audio.clip != null && !Paused && Mathf.Approximately(_audio.time, _audio.clip.length);

		public bool Paused { get; set; }

		public bool IsPlaying => _audio.isPlaying;
		public bool IsStopped => !IsPlaying && _audio.time == 0;
		public void Stop()
		{
			if (!Settings.PlayCutscenes || _audio == null || _audio.clip == null)
				return;

			if (!Paused)
				Sounds.SlideVolume(_audio, 1, 0, true);
			Paused = false;
		}

		public void Resume()
		{
			_audio.loop = false;
			_audio.time = _pauseTime - 3;
			Sounds.SlideVolume(_audio, 2, 1);
			Paused = false;
		}

		public void Pause()
		{
			if (_audio == null || Paused)
				return;

			_pauseTime = _audio.time;
			Sounds.SlideVolume(_audio, 2, 0.00021f);
			_audio.loop = true;
			Paused = true;
		}

		public void Play(string cutSceneName)
		{
			if (!Settings.PlayCutscenes)
				return;

			_audio.clip = Sounds.GetClip(cutSceneName);
			_audio.spatialize = false;
			_audio.spatialBlend = 0;
			_audio.volume = 1;
			_audio.Play();
			Paused = false;
		}

		private AudioSource _audio;
		private float _pauseTime;
	}
}
