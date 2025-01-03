﻿using Game.Entities.Characters;
using Game.Entities.Characters.Components;

namespace Game.Messaging.Commands.Characters
{
	/// <summary>
	/// instructs a component of an entity to react to pinching in a door.
	/// </summary>
	public class ReactToPinchingInDoor : Message
	{
		/// <summary>
		/// A component of an entity that sent the message.
		/// </summary>
		public new CharacterComponent Sender;

		/// <summary>
		/// An entity that tried to close the door.
		/// </summary>
		public Character Entity;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">A component of an entity that sent the message.</param>
		/// <param name="entity">An entity that tried to close the door.</param>
		public ReactToPinchingInDoor(CharacterComponent sender, Character entity) : base(sender)
		{
			Sender = sender;
			Entity = entity;
		}
	}
}