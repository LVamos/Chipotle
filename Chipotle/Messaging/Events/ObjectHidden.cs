
using Game.Entities;

namespace Game.Messaging.Events
{
    internal class ObjectHidden : GameMessage
    {
        public readonly GameObject Object;

        public ObjectHidden(object sender, GameObject o) : base(sender) => Object = o;

    }
}
