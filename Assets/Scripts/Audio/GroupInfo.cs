using System;

using UnityEngine.Audio;

namespace Game.Audio
{
	public class GroupInfo
	{
		public readonly AudioMixerGroup Group;
		public readonly int Id;

		public GroupInfo(AudioMixerGroup group, int id)
		{
			Group = group ?? throw new ArgumentNullException(nameof(group));
			Id = id;
		}
	}
}
