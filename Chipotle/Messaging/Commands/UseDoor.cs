using Game.Entities;

using OpenTK;

using System;

namespace Game.Messaging.Commands
{
    public class UseDoor : GameMessage
    {
        /// <summary>
        /// The character using the door
        /// </summary>
        public new readonly Character Sender;

        /// <summary>
        /// The manipulation point at which the character is using the door.
        /// </summary>
        public readonly Vector2 ManipulationPoint;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">The character using the door</param>
        /// <param name="manipulationPoint">The manipulation point at which the character is using the door.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sender"/> is null</exception>
        public UseDoor(Character sender, Vector2 manipulationPoint) : base(sender)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            ManipulationPoint = manipulationPoint;
        }
    }
}
