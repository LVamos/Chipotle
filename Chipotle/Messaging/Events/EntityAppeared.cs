using System.Collections.Generic;

using Game.Entities;
using Game.Messaging;

namespace Game.Messaging.Events
{
    public class EntityAppeared : GameMessage
    {

        public readonly Entity NewEntity;

        public EntityAppeared(object sender, Entity newEntity) : base(sender) => NewEntity = newEntity;

        public override bool Equals(object obj) => obj is EntityAppeared appeared && base.Equals(obj) && EqualityComparer<object>.Default.Equals(Sender, appeared.Sender) && EqualityComparer<Entity>.Default.Equals(NewEntity, appeared.NewEntity);

        public override int GetHashCode()
        {
            int hashCode = -859682016;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Sender);
            hashCode = hashCode * -1521134295 + EqualityComparer<Entity>.Default.GetHashCode(NewEntity);
            return hashCode;
        }
    }
}
