using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
	public class GroupPool : MonoBehaviour
	{
		public void ReleaseGroup(AudioMixerGroup group)
		{
			// Get id of the used group
			if (group == null)
				throw new ArgumentNullException(nameof(group), "Cannot release null group");
			int id;
			if (!_usedGroups.TryGetValue(group, out id))
				throw new InvalidOperationException($"Group {group.name} is not in use or has already been released.");

			// Remove group from used groups and add it back to available groups
			_usedGroups.Remove(group);
			if (_groups.ContainsKey(id))
				throw new InvalidOperationException($"Group {group.name} with id {id} is already in the pool.");
			_groups[id] = group;
		}

		// Write a method that sets the Cutoff parameer in the _mixer. Constrct the parameter name from Cutoff and group id. At first, get id of the group from _usedGroups.
		public void SetCutoff(AudioMixerGroup group, float cutoff)
		{
			if (group == null)
				throw new ArgumentNullException(nameof(group), "Cannot set cutoff for null group");
			if (!_usedGroups.TryGetValue(group, out int id))
				throw new InvalidOperationException($"Group {group.name} is not in use or has already been released.");
			string parameterName = $"Cutoff{id}";
			_mixer.SetFloat(parameterName, cutoff);
		}

		/// <summary>
		///		Returns the cutoff value for the specified group.
		/// </summary>
		/// <param name="group">Reference to the group</param>
		/// <returns>Value of the Cutoff parameter</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public float GetCutoff(AudioMixerGroup group)
		{
			if (group == null)
				throw new ArgumentNullException(nameof(group), "Cannot get cutoff for null group");
			if (!_usedGroups.TryGetValue(group, out int id))
				throw new InvalidOperationException($"Group {group.name} is not in use or has already been released.");
			string parameterName = $"Cutoff{id}";
			if (!_mixer.GetFloat(parameterName, out float cutoff))
				throw new InvalidOperationException($"Parameter {parameterName} not found in the mixer.");
			return cutoff;
		}

		public AudioMixerGroup GetGroup()
		{
			if (_groups.Count == 0)
				throw new InvalidOperationException($"No awailable groups in {nameof(_groups)}");

			int id = _groups.Keys.First();
			AudioMixerGroup group = _groups[id];
			_usedGroups[group] = id;
			_groups.Remove(id);
			return group;
		}

		private UnityEngine.Audio.AudioMixer _mixer;

		public void Activate(UnityEngine.Audio.AudioMixer mixer)
		{
			_mixer = mixer ?? throw new NullReferenceException(nameof(mixer));
			LoadGroups();
		}

		private const string _resonanceGroupName = "Resonance";
		private const int _groupCount = 30;
		private const string _muffledGroupPrefix = "Muffled";
		public AudioMixerGroup ResonanceGroup { get; private set; }
		private void LoadGroups()
		{
			ResonanceGroup = LoadGroup(_resonanceGroupName);

			for (int i = 1; i < 31; i++)
			{
				string name = _muffledGroupPrefix + i.ToString();
				_groups[i] = LoadGroup(name);
			}
		}

		private AudioMixerGroup LoadGroup(string name)
		{
			const string resonanceGroup = "Resonance";
			AudioMixerGroup group = _mixer.FindMatchingGroups(name).FirstOrDefault() ?? throw new InvalidOperationException($"{resonanceGroup} mixer group not loaded");
			return group;
		}


		private Dictionary<int, AudioMixerGroup> _groups = new(_groupCount);
		private Dictionary<AudioMixerGroup, int> _usedGroups = new(_groupCount);
	}
}
