using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

namespace Game.Entities
{
    /// <summary>
    /// Controls the behavior of an NPC.
    /// </summary>
    [Serializable]
    public class AIComponent : EntityComponent
    {
        /// <summary>
        /// Area occupied by the NPC
        /// </summary>
        protected Plane _area;

        /// <summary>
        /// Instance of a path finder
        /// </summary>
        protected PathFinder _finder = new PathFinder();

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case PositionChanged pc: OnPositionChanged(pc); break;
                default: base.HandleMessage(message); break;
            }
    }

        /// <summary>
        /// Processes the PositionChanged message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnPositionChanged(PositionChanged message)
            => _area = message.TargetPosition;
    }
}