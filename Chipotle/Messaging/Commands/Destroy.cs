using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Destroys an NPC or object.
    /// </summary>
    /// <remarks>Applies to all descendants of the <see cref="Game.Entities.GameObject"/> class.</remarks>
    [Serializable]
    public class Destroy : GameMessage
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public Destroy(object sender) : base(sender)
        {
        }
    }
}