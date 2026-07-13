using Game.Terrain;

namespace Game.Messaging.Events.Movement
{
    public class ObjectGotBehindPlayer : Message
    {
        public readonly MapElement Object;

        public ObjectGotBehindPlayer(object sender, MapElement obj) : base(sender)
        {
            Object = obj;
        }
    }
}
