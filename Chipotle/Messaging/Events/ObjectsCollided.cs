using Game.Terrain;

namespace Game.Messaging.Events
{
    internal class ObjectsCollided : GameMessage
    {
        public readonly Tile Tile;

        /// <summary>
        /// Constructs new instance of the message.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="tile">Tile with the colliding object</param>
        public ObjectsCollided(object sender, Tile tile) : base(sender) => Tile = tile;
    }
}
