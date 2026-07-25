using Game.Audio;

using UnityEngine;

namespace Game.Entities.Characters.Chipotle
{
    internal class Sonar
    {
        public void SetState(bool leftEnabled, bool rightEnabled)
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
        }

        public bool Playing
            => (_leftSource != null && _leftSource.isPlaying) || (_rightSource != null && _rightSource.isPlaying);

        private AudioSource _leftSource;
        private AudioSource _rightSource;

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

        private void PlayLeft()
            => _leftSource = PlayLoop(-0.75f);

        private void PlayRight()
            => _rightSource = PlayLoop(0.75f);

        private AudioSource PlayLoop(float panning)
        {
            AudioSource source = Sounds.Play2d(Settings.SonarSoundName, Settings.SonarVolume, true);
            source.panStereo = panning;
            return source;
        }
    }
}
