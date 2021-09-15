using System;
using System.Collections.Generic;

using OpenTK;

namespace Game.Messaging.Commands
{
    public class GotoPoint : GameMessage
    {
        public readonly Queue<Vector2> Path;
        public readonly int StepLength;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sender">Creator of the message</param>
        /// <param name="path">List of points of the path to the target</param>
        /// <param name="stepLength">Length of one step in milliseconds</param>
        public GotoPoint(object sender, Queue<Vector2> path, int stepLength) : base(sender)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            StepLength = stepLength;
        }
    }
}