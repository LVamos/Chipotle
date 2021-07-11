using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Entities;

namespace Game.Terrain
{
	public class EntityAppeared: Message
	{
		public readonly Entity NewEntity;

		public EntityAppeared(object sender, Entity newEntity) : base(sender) => NewEntity = newEntity;
	}
}
