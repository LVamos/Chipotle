using Game.Entities;
using Game.Terrain;

namespace Game.Messaging.Events
{
    public class ChipotlesCarMoved : GameMessage
    {
        public readonly Plane TargetLocation;
        public readonly ChipotlesCar Car;

        public ChipotlesCarMoved(ChipotlesCar sender, Plane targetLocation) : base(sender)
            => TargetLocation = targetLocation;
    }
}
