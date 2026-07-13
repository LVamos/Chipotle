using Game.Terrain;

namespace Game.Messaging.Events.Movement
{
    public class ObjectStoppedBeingBehindPlayer : Message
    {
        public readonly MapElement Object;

        public ObjectStoppedBeingBehindPlayer(object sender, MapElement obj) : base(sender)
        {
            Object = obj;
        }
    }
}
