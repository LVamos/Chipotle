
using Game.Terrain;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC changed its position.
    /// </summary>
    /// <remarks>sent from the <see cref="Game.Entities.Entity"/> class.</remarks>
    public class EntityMoved : GameMessage
    {
        /// <summary>
        /// New position of the NPC
        /// </summary>
        public readonly Tile Target;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="target">New position of the NPC</param>
        public EntityMoved(object sender, Tile target) : base(sender) => Target = target;

    }
}
