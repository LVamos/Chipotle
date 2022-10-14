using OpenTK;

using System.Collections.Generic;

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
