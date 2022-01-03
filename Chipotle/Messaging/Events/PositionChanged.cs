using System;

using Game.Terrain;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC changed its position.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    [Serializable]
    public class PositionChanged : GameMessage
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
        /// <param name="sourcePosition">Original position of the NPC</param>
        /// <param name="targetPosition">New position of the NPC</param>
        public PositionChanged(object sender, Plane sourcePosition, Plane targetPosition, Locality sourceLocality, Locality targetLocality) : base(sender)
        {
            if (sourcePosition != null)
                SourcePosition = new Plane(sourcePosition);

            TargetPosition = new Plane(targetPosition);
            SourceLocality = sourceLocality;
            TargetLocality = targetLocality;
        }
    }
}