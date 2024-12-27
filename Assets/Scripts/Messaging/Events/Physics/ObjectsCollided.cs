using Game.Entities;

using System;

using UnityEngine;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Indicates collision between an NPC and an object.
	/// </summary>
	/// <remarks>Sent from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.</remarks>
	[Serializable]
	public class ObjectsCollided : Message
	{
		/// <summary>
		/// The colliding object
		/// </summary>
		public readonly Entity Object;

		/// Position of the tile on which part of the used object lays. It should be always in front
		/// of the NPC. </summary>
		public readonly Vector2 Position;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="collidingObject">The colliding object</param>
		/// <param name="position">POint of the collision</param>
		/// <param name="tile">The tile under the object the NPC bumped to</param>
		public ObjectsCollided(object sender, Entity collidingObject, Vector2 position) : base(sender)
		{
			Object = collidingObject;
			Position = position;
		}
	}
}