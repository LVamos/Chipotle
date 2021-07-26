using Game.Terrain;

namespace Game.Messaging.Events
{
    public class PositionChanged : GameMessage
    {
        public readonly Plane Area;

        public PositionChanged(object sender, Plane area) : base(sender)
            => Area = new Plane(area);
    }
}
