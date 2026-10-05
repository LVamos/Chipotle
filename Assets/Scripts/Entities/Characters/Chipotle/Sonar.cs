using Game.Audio;

using UnityEngine;

namespace Game.Entities.Characters.Chipotle
{
	internal class Sonar
	{
		public void SetState(bool leftEnabled = false, bool rightEnabled = false, bool frontEnabled = false)
		{
			if (leftEnabled)
			{
				if (_leftSource == null)
					PlayLeft();
			}
			else StopLeft();

			if (rightEnabled)
			{
				if (_rightSource == null)
					PlayRight();
			}
			else StopRight();

			if (frontEnabled)
			{
				if (_frontSource == null)
					PlayFront();
			}
			else StopFront();
		}

		public bool Playing
			=> (_leftSource != null && _leftSource.isPlaying)
			|| (_rightSource != null && _rightSource.isPlaying)
			|| (_frontSource != null && _frontSource.isPlaying);

		private AudioSource _leftSource;
		private AudioSource _rightSource;
		private AudioSource _frontSource;

		private void StopLeft()
		{
			_leftSource?.Stop();
			_leftSource = null;
		}

		private void StopRight()
		{
			_rightSource?.Stop();
			_rightSource = null;
		}

		private void StopFront()
		{
			_frontSource?.Stop();
			_frontSource = null;
		}

		private void PlayLeft()
			=> _leftSource = PlayLoop(-0.75f);

		private void PlayRight()
			=> _rightSource = PlayLoop(0.75f);

		private void PlayFront()
			=> _frontSource = PlayLoop(0, Settings.SonarFrontPitch);

		private AudioSource PlayLoop(float panning, float pitch = 1)
		{
			AudioSource source = Sounds.Play2d(Settings.SonarSoundName, Settings.SonarVolume, true);
			source.panStereo = panning;
			source.pitch = pitch;
			return source;
		}
	}
}
