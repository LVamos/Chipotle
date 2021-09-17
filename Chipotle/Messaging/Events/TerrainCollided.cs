
using Game.Terrain;
namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC has attempted to enter impenetrable terrain.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    public class TerrainCollided : GameMessage
    {
        /// <summary>
        /// Tile with the impermeable terrain
        /// </summary>
        public readonly Tile Tile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="tile">Tile with the impermeable terrain</param>
        public TerrainCollided(object sender, Tile tile) : base(sender) => Tile = tile;
    }
}
