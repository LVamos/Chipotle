using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Terrain;

using System;

using UnityEngine;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Informs a character that one or two objects were used.
	/// </summary>
	[Serializable]
	public class ObjectsUsed : Message
	{
		public Character Character { get; }

		/// <summary>
		/// The character that wants to use some objects.
		/// </summary>
		public new readonly object Sender;

		/// <summary>
		/// A point of the object at which it's used
		/// </summary>
		public readonly Vector2 ManipulationPoint;

		/// <summary>
		/// The item directly used by the character
		/// </summary>
		public readonly Item UsedObject;

		/// <summary>
		/// The other item used by a character through another object
		/// </summary>
		public readonly MapElement Target;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">A character using the objects</param>
		/// <param name="manipulationPoint">A point of the object at which it's used</param>
		/// <param name="usedObject">The item directly used by the character</param>
		/// <param name="target">The other item or character used by a character through another object</param>
		public ObjectsUsed(object sender, Character character, Vector2 manipulationPoint, Item usedObject, MapElement target = null) : base(sender)
		{
			Sender = sender;
			Character = character;
			ManipulationPoint = manipulationPoint;
			UsedObject = usedObject;
		}
	}
}
