using Game.Messaging.Events.Sound;

using UnityEngine;

namespace Game.Audio
{
	public class CutscenePlayer : MonoBehaviour
	{
		private object _sender;

		private string _cutsceneName;

		private bool _wasPlaying;

		private void Update()
		{
			if (_audio.clip == null)
				return;

			if (_audio.isPlaying)
			{
				_wasPlaying = true;
				return;
			}

			if (_wasPlaying)
			{
				_wasPlaying = false;

				World.TakeMessage(
					new CutsceneEnded(_sender, _cutsceneName)
				);

				_audio.clip = null;
				_sender = null;
			}
		}

		public float Volume => _audio.volume; public float Position => _audio.time;

		public void Rewind(int seconds)
		{
			if (_audio == null)
				return;

			if (Paused || _audio.isPlaying)
				if (_audio.time > seconds)
					_audio.time -= seconds;
		}

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

			// Announce end of playback
			CutsceneEnded message = new(_sender, _cutsceneName);
			World.TakeMessage(message);
		}

		public void Resume()
		{
			if (!Settings.PlayCutscenes || _audio == null)
				return;

			if (!_audio.isPlaying)
				_audio = Sounds.Play2d(_cutsceneName, 0);

			_audio.time = _pauseTime - 3;
			Sounds.SlideVolume(_audio, 2, 1);
			Paused = false;
		}

		public void Pause()
		{
			if (_audio == null || Paused)
				return;

			_pauseTime = _audio.time;
			Sounds.SlideVolume(_audio, 2, 0);
			Paused = true;
		}

		public void Play(string cutsceneName, object sender)
		{
			if (!Settings.PlayCutscenes)
				return;

			_sender = sender;
			_cutsceneName = cutsceneName;
			_audio = Sounds.Play2d(cutsceneName);
			Paused = false;
			_wasPlaying = false;

			// Announce playback
			CutsceneBegan message = new(this, cutsceneName);
			World.TakeMessage(message);
		}

		private AudioSource _audio;
		private float _pauseTime;
	}
}
