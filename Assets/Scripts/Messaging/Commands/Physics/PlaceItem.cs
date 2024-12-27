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
		/// <summary>
		/// The object that should be put on the ground.
		/// </summary>
		public readonly Item Item;

		/// <summary>
		/// A point near which the object should be placed.
		/// </summary>
		public readonly Vector2? Position;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="item">The object that should be put on the ground</param>
		/// <param name="position">A point near which the object should be placed</param>
		/// <exception cref="ArgumentNullException">Throws an exception if item is null.</exception>
		public PlaceItem(object sender, Item item, Vector2? position = null) : base(sender)
		{
			Item = item ?? throw new ArgumentNullException(nameof(item));
			Position = position;
		}
	}
}