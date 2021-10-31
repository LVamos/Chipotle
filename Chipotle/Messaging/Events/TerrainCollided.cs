using System;

using Game.Terrain;

using OpenTK;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC has attempted to enter impenetrable terrain.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    [Serializable]
    public class TerrainCollided : GameMessage
    {
        /// Position of the tile on which part of the colliding object lays. It should be always in
        /// front of the NPC. </summary>
        public readonly Vector2 Position;

        /// <summary>
        /// Tile with the impermeable terrain
        /// </summary>
        public readonly Tile Tile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="tile">Tile with the impermeable terrain</param>
        public TerrainCollided(object sender, Vector2 position, Tile tile) : base(sender)
        {
            Position = position;
            Tile = tile;
        }
    }
}