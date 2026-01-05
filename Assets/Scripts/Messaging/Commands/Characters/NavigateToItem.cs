using Game.Entities.Items;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands.Characters
{
	public class NavigateToItem:Message
	{
		public readonly Item Item;

		public NavigateToItem(object sender, Item item) : base(sender)
		{
			Item = item;
		}

	}
}
