using Game.Terrain;

using System;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC changed its position.
    /// </summary>
    /// <remarks>sent from the <see cref="Game.Entities.Character"/> class.</remarks>
    [Serializable]
    public class CharacterMoved : GameMessage
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
        public readonly Rectangle SourcePosition;

        /// <summary>
        /// New position of the NPC
        /// </summary>
        public readonly Rectangle TargetPosition;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="sourcePosition">Source position fo the NPC</param>
        /// <param name="targetPosition">Target position of the NPC</param>
        /// <param name="sourceLocality">Source locality of the NPC</param>
        /// <param name="targetLocality">Target locality of the NPC</param>
        public CharacterMoved(object sender, Rectangle sourcePosition, Rectangle targetPosition, Locality sourceLocality, Locality targetLocality) : base(sender)
        {
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
            SourceLocality = sourceLocality;
            TargetLocality = targetLocality;
        }
    }
}