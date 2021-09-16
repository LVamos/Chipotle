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
    public class AIComponent : EntityComponent
    {
        protected Plane _area;
        protected PathFinder _finder = new PathFinder();

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
                    [typeof(PositionChanged)] = (m) => OnPositionChanged((PositionChanged)m)
                }
                );
        }

        /// <summary>
        /// Processes the PositionChanged message.
        /// </summary>
        /// <param name="m">The message to be processed</param>
        protected void OnPositionChanged(PositionChanged m)
            => _area = m.Area;
    }
}