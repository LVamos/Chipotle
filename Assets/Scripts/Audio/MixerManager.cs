using System;
using System.Linq;

using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
	public class MixerManager : MonoBehaviour
	{
		public AudioMixerGroup SpatializedGroup { get; private set; }

		private void Awake()
			=> LoadMixer();

		private const string _mixerName = "MyMixer";
		private const string _spatializedGroupName = "Spatialized";

		private void LoadMixer()
		{
			_mixer = Resources.Load<AudioMixer>(_mixerName) ?? throw new InvalidOperationException($"{_mixerName} mixer not loaded");
			SpatializedGroup = _mixer.FindMatchingGroups(_spatializedGroupName).FirstOrDefault()
				?? throw new InvalidOperationException("Mixer group not found");
		}




		private UnityEngine.Audio.AudioMixer _mixer;
	}
}
