using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells a dump object to stop ongoing sound navigation.
    /// </summary>
    [Serializable]
    public class StopObjectNavigation : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public StopObjectNavigation(object sender) : base(sender) { }
    }
}
