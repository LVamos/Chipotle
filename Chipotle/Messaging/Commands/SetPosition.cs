
using Game.Terrain;

namespace Game.Messaging.Commands
{
    public class SetPosition : GameMessage
    {
        public readonly Plane Target;

        public SetPosition(object sender, Plane target) : base(sender)
            => Target = target;

    }
}
