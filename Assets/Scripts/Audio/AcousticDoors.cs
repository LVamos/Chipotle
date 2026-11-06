using Game.Terrain;

using SteamAudio;

using System.Collections.Generic;

using UnityEngine;

namespace Game.Audio
{
	/// <summary>
	/// Keeps the Steam Audio geometry of doors in sync with the game. A door swings open or shut over a while, as in reality:
	/// it seals the doorway for Steam Audio only once it is fully closed, and its collider, which <see cref="Assets.Scripts.Audio.DoorTransmission"/>
	/// traces the sound falling on it with, stays until it is fully open.
	/// </summary>
	/// <remarks>Children of this object are named after the passages in the map and carry a SteamAudioDynamicObject.
	/// Runs before the Steam Audio manager, so a door sound played in the same frame is simulated with the door already opened or closed.</remarks>
	[DefaultExecutionOrder(-1000)]
	public class AcousticDoors : MonoBehaviour
	{
		/// <summary>
		/// Name of the object created by the editor script.
		/// </summary>
		public const string ObjectName = "Steam Audio doors";

		/// <summary>
		/// How far a door is open, from 0 (closed) to 1 (open), in proportion to the area of the doorway it leaves free.
		/// </summary>
		public static float GetOpenness(Transform door)
			=> _openness.TryGetValue(door, out float openness) ? openness : 1;

		private static readonly Dictionary<Transform, float> _openness = new();

		private void OnDestroy() => _openness.Clear();

		private void LateUpdate()
		{
			if (World.Player == null)
				return; // The map is not loaded yet.

			bool changed = false;
			foreach (Transform door in transform)
			{
				if (World.GetPassage(door.name) is not Passage passage)
					continue;

				bool open = passage.State == PassageState.Open;
				float target = open ? 1 : 0;
				if (!_openness.TryGetValue(door, out float openness))
				{
					openness = target;
					door.gameObject.SetActive(true);
				}

				float duration = open ? Settings.DoorOpeningOcclusionDuration : Settings.DoorClosingOcclusionDuration;
				openness = Mathf.MoveTowards(openness, target, Time.deltaTime / duration);
				_openness[door] = openness;

				door.GetComponent<Collider>().enabled = openness < 1;
				SteamAudioDynamicObject geometry = door.GetComponent<SteamAudioDynamicObject>();
				bool sealing = openness <= 0;
				if (geometry.enabled != sealing)
				{
					geometry.enabled = sealing;
					changed = true;
				}
			}

			if (changed)
				SpatialAudio.NotifyGeometryChanged();
		}
	}
}
