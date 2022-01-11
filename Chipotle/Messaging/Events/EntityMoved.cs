using System;

using Game.Entities;
using Game.Terrain;

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
        /// <summary>
        /// The locality in which the NPC was originally located.
        /// </summary>
        public readonly Locality SourceLocality;

        /// <summary>
        /// The locality in which the NPC is currently located.
        /// </summary>
        public readonly Locality TargetLocality;

        /// <summary>
        /// Original position of the NPC
        /// </summary>
        public readonly Plane SourcePosition;

        /// <summary>
        /// New position of the NPC
        /// </summary>
        public readonly Plane TargetPosition;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="sourcePosition">Source position fo the NPC</param>
        /// <param name="targetPosition">Target position of the NPC</param>
        /// <param name="sourceLocality">Source locality of the NPC</param>
        /// <param name="targetLocality">Target locality of the NPC</param>
        public EntityMoved(object sender, Plane sourcePosition, Plane targetPosition, Locality sourceLocality, Locality targetLocality) : base(sender)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            SourceLocality = sourceLocality;
            TargetLocality = targetLocality;
        }
    }
}