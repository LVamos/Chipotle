using System;

namespace Game.Messaging
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    [Serializable]
    public abstract class GameMessage
    {
        /// <summary>
        /// Source of the message
        /// </summary>
        public readonly object Sender;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public GameMessage(object sender)
            => Sender = sender;

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
            => unchecked(7984 * (1357 + Sender.GetHashCode()));
    }
}