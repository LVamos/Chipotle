using UnityEngine;

namespace Game
{
	public static class AudioSourceExtensions
	{
		public static bool IsPlaying(this AudioSource source) => source != null && source.isPlaying;
	}
}
