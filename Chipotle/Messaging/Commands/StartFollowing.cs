using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells the Tuttle NPC to follow the Detective Chipotle NPC.
    /// </summary>
    /// <remarks>
    /// Can be sent only from inside the NPC from <see cref="Game.Entities.CharacterComponent"/> class.
    /// </remarks>
    [Serializable]
    public class StartFollowing : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public StartFollowing(object sender) : base(sender)
        { }
    }
}