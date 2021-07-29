
using Game.Terrain;

namespace Game.Messaging.Commands
{
    public class SetPosition : GameMessage
    {
        public readonly Plane Target;
        public readonly bool Silently;

        public SetPosition(object sender, Plane target, bool silently=false) : base(sender)
        {
            Target = target;
            Silently = silently;
        }

    }
}
