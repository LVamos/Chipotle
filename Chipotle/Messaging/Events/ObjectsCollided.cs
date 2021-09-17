using Game.Terrain;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates collision between an NPC and an object.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    public class ObjectsCollided : GameMessage
    {
        /// <summary>
        /// The tile under the object the NPC bumped to
        /// </summary>
        public readonly Tile Tile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="tile">The tile under the object the NPC bumped to</param>
        public ObjectsCollided(object sender, Tile tile) : base(sender) => Tile = tile;
    }
}
