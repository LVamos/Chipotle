﻿using System;

namespace Game.Messaging.Commands.GameInfo
{
	[Serializable]
	public class SayLocalityName : Message
	{
		/// <summary>
		/// Instructs the NPC to report the name of the locality in which it is currently located.
		/// </summary>
		/// <remarks>
		/// Applies to the <see cref="Entities.Characters.Character"/> class. Can be sent only from inside
		/// the NPC from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.
		/// </remarks>
		public SayLocalityName(object sender) : base(sender)
		{
		}
	}
}