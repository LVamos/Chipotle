using Game.Terrain;

using UnityEngine;

namespace Game.Audio
{
	/// <summary>
	/// Keeps the Steam Audio geometry of doors in sync with the game: a closed or locked door blocks the sound, an open one is removed.
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

		private void LateUpdate()
		{
			if (World.Player == null)
				return; // The map is not loaded yet.

			bool changed = false;
			foreach (Transform door in transform)
			{
				if (World.GetPassage(door.name) is not Passage passage)
					continue;

				bool closed = passage.State != PassageState.Open;
				if (door.gameObject.activeSelf != closed)
				{
					door.gameObject.SetActive(closed);
					changed = true;
				}
			}

			if (changed)
				SpatialAudio.NotifyGeometryChanged();
		}
	}
}
