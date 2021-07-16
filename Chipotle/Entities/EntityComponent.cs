using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;

using Luky;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Entities
{
    public  abstract  class EntityComponent: MessagingObject
    {



        //todo EntityComponent



        public Entity Owner;



public new Name Name { get => Owner?.Name; }
    }
}
