using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells a passage to stop ongoing sound navigation.
    /// </summary>
    [Serializable]
    public class StopExitNavigation : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public StopExitNavigation(object sender) : base(sender) { }
    }
}
