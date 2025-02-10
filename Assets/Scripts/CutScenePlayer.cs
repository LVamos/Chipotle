using Game;
using Game.Audio;

using System.Collections;

using UnityEngine;

namespace Assets.Scripts
{
	public class CutScenePlayer : MonoBehaviour
	{
		public float Volume => _audio.volume;
		public void FadeIn(float duration, float volume)
		{
			if (_audio == null)
				return;

			_audio.volume = 0;
			AdjustVolume(duration, volume);
		}
		public float Position => _audio.time;

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
		protected void AdjustVolume(float duration, float targetVolume)
		{
			if (targetVolume == _audio.volume)
				return;

			StartCoroutine(Ajust());

			IEnumerator Ajust()
			{
				float startVolume = _audio.volume;

				for (float t = 0; t < duration; t += Time.deltaTime)
				{
					_audio.volume = Mathf.Lerp(startVolume, targetVolume, t / duration);
					yield return null;
				}
				_audio.volume = targetVolume;

				if (targetVolume <= 0)
					_audio.Stop();
			}
		}

		public bool IsPlaying => _audio.isPlaying;

		public void Stop()
		{
			if (!Settings.PlayCutscenes)
				return;

			if (Paused)
				_audio.Stop();
			AdjustVolume(.5f, 0);
			Paused = false;
		}

		public void Resume()
		{
			if (_audio == null)
				return;

			_audio.UnPause();
			Paused = false;
		}

		public void Pause()
		{
			if (_audio == null || Paused)
				return;

			_audio.Pause();
			Paused = true;
		}

		public void Play(string cutSceneName)
		{
			if (!Settings.PlayCutscenes)
				return;

			_audio = Sounds.Play2d(cutSceneName);
			Paused = false;

		}

		private AudioSource _audio;
	}
}
