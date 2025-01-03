﻿using System;
using Game.Terrain;

namespace Game.Messaging.Commands.Movement
{
	/// <summary>
	/// Tells an NPC to make one step in the specified direction.
	/// </summary>
	/// <remarks>
	/// Applies to the <see cref="Entities.Characters.Character"/> class. Can be sent only from inside the
	/// NPC from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.
	/// </remarks>
	[Serializable]
	public class StartWalk : ChangeOrientation
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="direction">The direction in which the entity should move</param>
		/// <remarks><see cref="TurnType.None"/> will cause the NPC to move forward.</remarks>
		public StartWalk(object sender, TurnType direction) : base(sender, direction) { }
	}
}