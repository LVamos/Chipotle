using OpenTK;

using System;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC has attempted to enter impenetrable terrain.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.</remarks>
    [Serializable]
    public class TerrainCollided : GameMessage
    {
        /// <summary>
        /// Position of the tile on which part of the colliding object lays. It should be always in
        /// front of the NPC
        /// </summary>
        public readonly Vector2 Position;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="position">Position of the colliding terrain</param>
        public TerrainCollided(object sender, Vector2 position) : base(sender)
            => Position = position;
    }
}