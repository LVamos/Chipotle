﻿using System;

namespace Game.Messaging.Commands.Movement
{
	/// <summary>
	/// Tells the Tuttle NPC to follow the Detective Chipotle NPC.
	/// </summary>
	/// <remarks>
	/// Can be sent only from inside the NPC from <see cref="Entities.Characters.Components.CharacterComponent"/> class.
	/// </remarks>
	[Serializable]
	public class StartFollowingPlayer : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public StartFollowingPlayer(object sender) : base(sender)
		{ }
	}
}