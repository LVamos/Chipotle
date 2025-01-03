﻿using Game.Entities;

using System;

using UnityEngine;

namespace Game.Messaging.Commands.Movement
{
	/// <summary>
	/// Tells an NPC or object to walk on a specified route in a specified speed.
	/// </summary>
	/// <remarks>Applies to all descendants of the <see cref="Entity"/> class.</remarks>
	[Serializable]
	public class GotoPoint : Message
	{
		/// <summary>
		/// End of the path
		/// </summary>
		public readonly Vector2 Goal;

		/// <summary>
		/// Specifies if Tuttle should stop following the player while leading to the target.
		/// </summary>
		public readonly bool WatchPlayer;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="goal">The goal Tuttle should go to</param>
		/// <param name="stopFollowing">Specifies if Tuttle should stop following the player while leading to the target</param>
		public GotoPoint(object sender, Vector2 goal, bool stopFollowing = true) : base(sender)
		{
			Goal = goal;
			WatchPlayer = stopFollowing;
		}
	}
}