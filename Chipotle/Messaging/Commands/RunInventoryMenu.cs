using Game.Entities;

using System;

namespace Game.Messaging.Commands
{
    public class RunInventoryMenu : GameMessage
    {
        /// <summary>
        /// Source of the message.
        /// </summary>
        public new readonly CharacterComponent Sender;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RunInventoryMenu(CharacterComponent sender) : base(sender) => Sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }
}
