using Game.Terrain;

using System;

using UnityEngine;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Indicates that an NPC collided with a door object.
	/// </summary>
	/// <remarks>
	/// Sent from the <see cref="Entities.Characters.Character"/> class. Can be sent only from inside the NPC
	/// from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.
	/// </remarks>
	[Serializable]
	public class DoorHit : Message
	{
		public readonly string Destination;

		/// <summary>
		/// The door to which an NPC bumped.
		/// </summary>
		public readonly Door Door;

		/// <summary>
		/// The point at which the door was hit.
		/// </summary>
		public readonly Vector2 Point;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="door"></param>
		/// <param name="point"></param>
		public DoorHit(object sender, Door door, Vector2 point, string destination) : base(sender)
		{
			Door = door;
			Point = point;
			Destination = destination;
		}
	}
}