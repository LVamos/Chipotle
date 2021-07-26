
using Game.Terrain;
namespace Game.Messaging.Events
{
    internal class TerrainCollided : GameMessage
    {
        public readonly Tile Tile;

        /// <summary>
        /// Constructs new instance of InpermeableTerrainCollision.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="collidingTile">The inpermeable tile</param>
        public TerrainCollided(object sender, Tile tile) : base(sender) => Tile = tile;
    }
}
