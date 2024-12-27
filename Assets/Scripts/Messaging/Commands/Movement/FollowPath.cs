using System.Collections.Generic;

using UnityEngine;

namespace Game.Messaging.Commands.Movement
{
	public class FollowPath : Message
	{
		/// <summary>
		/// List of points to be followed.
		/// </summary>
		public readonly Queue<Vector2> Path;

		public FollowPath(object sender, Queue<Vector2> path) : base(sender)
			=> Path = path;
	}
}