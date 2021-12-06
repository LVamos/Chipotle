using System;

using Game.Terrain;

using OpenTK;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells an NPC to interact with an object.
    /// </summary>
    /// <remarks>
    /// Applies to the <see cref="Game.Entities.Entity"/> class. Can be sent only from inside the
    /// NPC from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.
    /// </remarks>
    [Serializable]
    public class UseObject : GameMessage
    {
        /// <summary>
        /// Position of the tile on which part of the used object lays. It should be always in front
        /// of the NPC.
        /// </summary>
        public readonly Vector2 Position;

        /// <summary>
        /// The tile on which part of the used object lays. It should be always in front of the NPC.
        /// </summary>
        public readonly Tile Tile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="position">Position of the tile on which part of the used object lays</param>
        /// <param name="tile">A tile with the object</param>
        public UseObject(object sender, Vector2 position, Tile tile = null) : base(sender)
        {
            Position = position;
            Tile = tile;
        }
    }
}