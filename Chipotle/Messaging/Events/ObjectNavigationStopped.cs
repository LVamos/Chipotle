using System;

namespace Game.Messaging.Events
{
    [Serializable]
    public class ObjectNavigationStopped : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public ObjectNavigationStopped(object sender) : base(sender) { }
    }
}
