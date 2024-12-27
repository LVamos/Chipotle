using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells a passage to start announcing its position with a sound loop.
    /// </summary>
    [Serializable]
    public class StartExitNavigation : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public StartExitNavigation(object sender) : base(sender) { }
    }
}
