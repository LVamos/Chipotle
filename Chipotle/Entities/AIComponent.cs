using System;
using System.Collections.Generic;
using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

namespace Game.Entities
{
    public class AIComponent : EntityComponent
    {
        protected  PathFinder _finder = new PathFinder();

        protected Plane _area;

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

        protected void OnPositionChanged(PositionChanged m)
            => _area = m.Area;


    }
}
