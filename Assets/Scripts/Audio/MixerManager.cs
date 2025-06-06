using System;
using System.Linq;

using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
	public class MixerManager : MonoBehaviour
	{
		public AudioMixerGroup ResonanceGroup { get; private set; }
		private void Awake()
		{
			LoadMixer();
		}


		private const string _mixerName = "MyMixer";
		private const string _resonanceGroupName = "Spatialized";

		private void LoadMixer()
		{
			_mixer = Resources.Load<AudioMixer>(_mixerName) ?? throw new InvalidOperationException($"{_mixerName} mixer not loaded");
			ResonanceGroup = _mixer.FindMatchingGroups(_resonanceGroupName).FirstOrDefault()
				?? throw new InvalidOperationException("Mixer group not found");
		}




		private UnityEngine.Audio.AudioMixer _mixer;
	}
}
