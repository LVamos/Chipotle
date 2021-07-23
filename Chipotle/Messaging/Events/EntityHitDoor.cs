using Luky;
using Game.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Events
{
    public class EntityHitDoor: GameMessage
    {

        public EntityHitDoor(object sender) : base(sender)
        {
        }
    }
}
