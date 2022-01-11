using System;
using System.Collections.Generic;

using OpenTK;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells an NPC or object to walk on a specified route in a specified speed.
    /// </summary>
    /// <remarks>Applies to all descendants of the <see cref="Game.Entities.GameObject"/> class.</remarks>
    [Serializable]
    public class GotoPoint : GameMessage
    {
        /// <summary>
        /// End of the path
        /// </summary>
        public readonly Vector2 Goal;

        /// <summary>
        /// Length of one step in milliseconds
        /// </summary>
        public readonly int StepLength;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="goal">The goal Tuttle should go to</param>
        /// <param name="stepLength">Length of one step in milliseconds</param>
        public GotoPoint(object sender, Vector2 goal, int stepLength) : base(sender)
        {
            Goal = goal;
            StepLength = stepLength;
        }
    }
}