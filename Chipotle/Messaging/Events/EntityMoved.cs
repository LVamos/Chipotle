using System;

using OpenTK;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC changed its position.
    /// </summary>
    /// <remarks>sent from the <see cref="Game.Entities.Entity"/> class.</remarks>
    [Serializable]
    public class EntityMoved : GameMessage
    {
        /// Position of the tile on which part of the used object lays. It should be always in front
        /// of the NPC. </summary>
        public readonly Vector2 Target;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="target">New position of the NPC</param>
        public EntityMoved(object sender, Vector2 target) : base(sender) => Target = target;
    }
}