using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Entities;

namespace Game.Messaging.Events
{
[Serializable]
    public class ExitNavigationStopped : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public ExitNavigationStopped(object sender) : base(sender) { }
    }
}
