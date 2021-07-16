using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Entities;

namespace Game.Terrain
{
	public class EntityAppeared: GameMessage
	{
		public readonly Entity NewEntity;

		public EntityAppeared(object sender, Entity newEntity) : base(sender) => NewEntity = newEntity;
	}
}
