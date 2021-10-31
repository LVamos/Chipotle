using System;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC collided with a door object.
    /// </summary>
    /// <remarks>
    /// Sent from the <see cref="Game.Entities.Entity"/> class. Can be sent only from inside the NPC
    /// from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.
    /// </remarks>
    [Serializable]
    public class DoorHit : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public DoorHit(object sender) : base(sender)
        {
        }
    }
}