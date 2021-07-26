using Game.Entities;
using Game.Messaging;

namespace Game.Terrain
{
    public class EntityAppeared : GameMessage
    {
        public readonly Entity NewEntity;

        public EntityAppeared(object sender, Entity newEntity) : base(sender) => NewEntity = newEntity;
    }
}
