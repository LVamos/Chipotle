using System;
using System.Collections.Generic;

using OpenTK;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells an NPC or object to walk on a specified route in a specified speed.
    /// </summary>
    /// <remarks>Applies to all descendants of the <see cref="Game.Entities.GameObject"/>  class.</remarks>
    public class GotoPoint : GameMessage
    {
        /// <summary>
        /// A preplanned route to walk on.
        /// </summary>
        public readonly Queue<Vector2> Path;

        /// <summary>
        /// Length of one step in milliseconds
        /// </summary>
        public readonly int StepLength;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="path">A preplanned route to walk on</param>
        /// <param name="stepLength">Length of one step in milliseconds</param>
        public GotoPoint(object sender, Queue<Vector2> path, int stepLength) : base(sender)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            StepLength = stepLength;
        }
    }
}