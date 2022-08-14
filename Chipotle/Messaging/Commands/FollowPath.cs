using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Game.Messaging.Commands
{
	public class FollowPath : GameMessage
	{
		/// <summary>
		/// List of points to be followed.
		/// </summary>
		public readonly Queue<Vector2> Path;

		public FollowPath(object sender, Queue<Vector2> path) : base(sender)
			=> Path = path;
	}
}
