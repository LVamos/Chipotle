using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells a dump object to start announcing its position with a sound loop.
    /// </summary>
    [Serializable]
    public class StartObjectNavigation : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public StartObjectNavigation(object sender) : base(sender) { }
    }
}
