using Game.Terrain;

using System;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC changed its position.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.</remarks>
    [Serializable]
    public class PositionChanged : CharacterMoved
    {
        /// <summary>
        /// Describes type of obstacle between the entity and the player if any.
        /// </summary>
        public readonly ObstacleType Obstacle;

        /// <summary>
        /// Indicates if foot steps of the NPC should be audible.
        /// </summary>
        public readonly bool Silently;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="sourcePosition">Source position of the NPC</param>
        /// <param name="targetPosition">Target position of the NPC</param>
        /// <param name="sourceLocality"Source locality of the NPC></param>
        /// <param name="targetLocality">Target locality of the NPC</param>
        /// <param name="obstacle">Describes type of obstacle between the entity and the player if any</param>
        /// <param name="silently">Determines if the fott steps of the NPC should be audible</param>
        public PositionChanged(object sender, Rectangle sourcePosition, Rectangle targetPosition, Locality sourceLocality, Locality targetLocality, ObstacleType obstacle = ObstacleType.None, bool silently = false) : base(sender, sourcePosition, targetPosition, sourceLocality, targetLocality)
        {
            Obstacle = obstacle;
            Silently = silently;

        }
    }
}