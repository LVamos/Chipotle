using Game.Entities;

using OpenTK;

using System;

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
    public class UseObjects : GameMessage
    {
        /// <summary>
        /// The character that wants to use some objects.
        /// </summary>
        public new readonly Character Sender;

        /// <summary>
        /// A point of the object at which it's used
        /// </summary>
        public readonly Vector2 ManipulationPoint;

        /// <summary>
        /// The item directly used by the character
        /// </summary>
        public readonly Item UsedObject;

        /// <summary>
        /// The other item used by a character through another object
        /// </summary>
        public readonly GameObject OtherObject;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">A character using the objects</param>
        /// <param name="manipulationPoint">A point of the object at which it's used</param>
        /// <param name="usedObject">The item directly used by the character</param>
        /// <param name="otherObject">The other item or character used by a character through another object</param>
        public UseObjects(Character sender, Vector2 manipulationPoint, Item usedObject, GameObject otherObject = null) : base(sender)
        {
            Sender = sender;
            ManipulationPoint = manipulationPoint;
            UsedObject = usedObject;
        }
    }
}