using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Terrain;

using System;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Places an item do the ground.
	/// </summary>
	public class PlaceItem : Message
	{
		public readonly Rectangle? Target;
		public readonly Character Character;

		/// <summary>
		/// The object that should be put on the ground.
		/// </summary>
		public readonly Item Item;

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
			Rectangle? target = null
			) : base(sender)
		{
			Item = item ?? throw new ArgumentNullException(nameof(item));
			Character = character;
			Target = target;
		}
	}
}