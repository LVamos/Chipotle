using System;

using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
	public class AudioMixer : MonoBehaviour
	{
		public AudioMixerGroup GetGroup() => _groupPool.GetGroup();
		public void ReleaseGroup(AudioMixerGroup group) => _groupPool.ReleaseGroup(group);
		public float GetCutoff(AudioMixerGroup group) => _groupPool.GetCutoff(group);
		public void SetCutoff(AudioMixerGroup group, float cutoff) => _groupPool.SetCutoff(group, cutoff);

		private void Awake()
		{
			LoadMixer();
			LoadGroups();
		}

		private void LoadGroups()
		{
			GameObject obj = new("Group pool");
			_groupPool = obj.AddComponent<GroupPool>();
			_groupPool.Activate(_mixer);
		}

		private GroupPool _groupPool;
		private const string _mixerName = "ResonanceMixer";

		private void LoadMixer()
		{
			AudioMixer mixer = Resources.Load<AudioMixer>(_mixerName) ?? throw new InvalidOperationException($"{_mixerName} mixer not loaded");
		}




		private UnityEngine.Audio.AudioMixer _mixer;

		public AudioMixerGroup ResonanceGroup { get => _groupPool.ResonanceGroup; }
	}
}
