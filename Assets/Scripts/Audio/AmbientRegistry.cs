using NUnit.Framework.Constraints;

using System.Collections.Generic;

using UnityEngine;
namespace Game.Audio
{
	public static class AmbientRegistry
	{
		private static readonly Dictionary<string, AudioSource> _zone2D = new();
		private static readonly Dictionary<string, HashSet<AudioSource>> _portals = new();

		public static void Register2D(string soundName, AudioSource source)
			=> _zone2D[soundName] = source;

		public static AudioSource TryGet2D(string soundName)
			=> _zone2D.TryGetValue(soundName, out var source) ? source : null;

		public static void Unregister2D(string soundName)
			=> _zone2D.Remove(soundName);

		public static void RegisterPortal(string soundName, AudioSource source)
		{
			if (!_portals.ContainsKey(soundName))
				_portals[soundName] = new();
			_portals[soundName].Add(source);
		}

		public static HashSet<AudioSource> TryGetPortals(string soundName)
		{
			HashSet<AudioSource> portals;
			return _portals.TryGetValue(soundName, out portals) ? portals : null;
		}

		public static void UnregisterPortals(string soundName)
			=> _portals.Remove(soundName);
		public static void UnregisterPortal(string soundName, AudioSource portal)
		{
			HashSet<AudioSource> portals = TryGetPortals(soundName);
			if (portals != null && portals.Contains(portal))
				portals.Remove(portal);
		}

		public static IEnumerable<string> Active2D => _zone2D.Keys;
		public static IEnumerable<string> ActivePortals => _portals.Keys;
	}
}