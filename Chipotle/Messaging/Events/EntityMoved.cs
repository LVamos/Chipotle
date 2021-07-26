
using Game.Terrain;

namespace Game.Messaging.Events
{
    internal class EntityMoved : GameMessage
    {
        public readonly Tile Target;


        /// <summary>
        /// Constructs new instance of the message.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="target">Final location</param>
        public EntityMoved(object sender, Tile target) : base(sender) => Target = target;

    }
}
