﻿using System;
using Game.Entities;

namespace Game.Messaging.Events.Characters
{
	/// <summary>
	/// Indicates that state of the Tuttle NPC was changed.
	/// </summary>
	[Serializable]
	public class CharacterStateChanged : Message
	{
		/// <summary>
		/// New state of the Tuttle NPC
		/// </summary>
		public readonly CharacterState State;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="state">New state of the NPC</param>
		public CharacterStateChanged(object sender, CharacterState state) : base(sender)
			=> State = state;
	}
}