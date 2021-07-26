using System;

namespace Game.Messaging
{

    public abstract class GameMessage
    {
        public override int GetHashCode()
            => unchecked(7984 * (1357 + Sender.GetHashCode()));

        public readonly object Sender;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public GameMessage(object sender)
            => Sender = sender ?? throw new ArgumentNullException(nameof(sender));

    }
}
