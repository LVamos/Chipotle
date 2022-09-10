using System;

using Game.Terrain;

using OpenTK;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells an NPC to interact with an object.
    /// </summary>
    /// <remarks>
    /// Applies to the <see cref="Game.Entities.Character"/> class. Can be sent only from inside the
    /// NPC from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.
    /// </remarks>
    [Serializable]
    public class UseObject : GameMessage
    {
        /// <summary>
        /// A point of the object at which it's used
        /// </summary>
        public readonly Vector2 ManipulationPoint;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="manipulationPoint">A point of the object at which it's used</param>
        /// <param name="tile">A tile with the object</param>
        /// <param name="obstacle">Describes type of obstacle between the entity and the player if any.</param>
        public UseObject(object sender, Vector2 manipulationPoint = default) : base(sender)
        {
            ManipulationPoint = manipulationPoint;
        }
    }
}