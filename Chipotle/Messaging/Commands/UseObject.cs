using Game.Terrain;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells an NPC to interact with an object.
    /// </summary>
    /// <remarks>Applies to the <see cref="Game.Entities.Entity"/> class. Can be sent only from inside the NPC from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    public class UseObject : GameMessage
    {
        /// <summary>
        /// The tile on which part of the used object lays. It should be always in front of the NPC.
        /// </summary>
        public readonly Tile Tile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public UseObject(object sender, Tile tile = null) : base(sender) => Tile = tile;

    }
}
