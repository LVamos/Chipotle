using Game.Entities;

using OpenTK;

using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Represents a command that causes that NPC puts an object on the ground.
    /// </summary>
    public class PlaceObject : GameMessage
    {
        /// <summary>
        /// Source of the message.
        /// </summary>
        public new readonly Character Sender;

        /// <summary>
        /// The object that should be put on the ground.
        /// </summary>
        public readonly Item Object;

        /// <summary>
        /// A point near which the object should be placed.
        /// </summary>
        public readonly Vector2 Position;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="object">The object that should be put on the ground</param>
        /// <param name="position">A point near which the object should be placed</param>
        /// <exception cref="ArgumentNullException">Throws an exception if one of the required parameters are null.</exception>
        public PlaceObject(Character sender, Item @object, Vector2 position) : base(sender)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Object = @object ?? throw new ArgumentNullException(nameof(@object));
            Position = position;
        }
    }
}
