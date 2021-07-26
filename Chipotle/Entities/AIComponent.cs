using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

namespace Game.Entities
{
    public class AIComponent : EntityComponent
    {
        protected Plane _area;

        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new System.Collections.Generic.Dictionary<System.Type, System.Action<GameMessage>>
                {
                    [typeof(PositionChanged)] = (m) => OnPositionChanged((PositionChanged)m)
                }
                );
        }

        protected void OnPositionChanged(PositionChanged m)
            => _area = m.Area;


    }
}
