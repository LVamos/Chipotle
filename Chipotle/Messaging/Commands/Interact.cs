using Game.Entities;

using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Instructs a physics component of a character to use an object.
    /// </summary>
    public class Interact : GameMessage
    {
        /// <summary>
        /// Source of the message
        /// </summary>
        public new readonly CharacterComponent Sender;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source fo the message</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sender"/> is null</exception>
        public Interact(CharacterComponent sender) : base(sender) => Sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }
}
