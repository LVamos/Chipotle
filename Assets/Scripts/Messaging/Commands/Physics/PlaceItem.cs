using Game.Entities.Characters;
using Game.Entities.Items;

using System;

using UnityEngine;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Places an item do the ground.
	/// </summary>
	public class PlaceItem : Message
	{
		public readonly Character Character;

		public readonly float MaxDistanceFromCharacter;
		/// <summary>
		/// The object that should be put on the ground.
		/// </summary>
		public readonly Item Item;

		/// <summary>
		/// A point near which the object should be placed.
		/// </summary>
		public readonly Vector2? DirectionFromCharacter;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="item">The object that should be put on the ground</param>
		/// <param name="directionFromCharacter">A point near which the object should be placed</param>
		/// <exception cref="ArgumentNullException">Throws an exception if item is null.</exception>
		public PlaceItem(
			object sender,
			Item item,
			Character character = null,
			Vector2? directionFromCharacter = null,
			float maxDistanceFromCharacter = 0
			) : base(sender)
		{
			Item = item ?? throw new ArgumentNullException(nameof(item));
			Character = character;
			DirectionFromCharacter = directionFromCharacter;
			MaxDistanceFromCharacter = maxDistanceFromCharacter;
		}
	}
}