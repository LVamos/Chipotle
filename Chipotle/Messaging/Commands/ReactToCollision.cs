using Game.Entities;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// A message that instruct an entity component to react on collision.
    /// </summary>
    public class ReactToCollision : GameMessage
    {
        /// <summary>
        /// The entity that bumped to this entity.
        /// </summary>
        public readonly Character entity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="entity">The entity that bumped to this entity</param>
        public ReactToCollision(object sender, Character entity) : base(sender) { }
    }
}
