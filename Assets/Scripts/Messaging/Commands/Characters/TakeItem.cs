using Game.Entities.Items;
using Game.Messaging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Messaging.Commands.Characters
{
	public class TakeItem: Message
	{
		public readonly Item Item;

		public TakeItem(object sender, Item item):base(sender)
		{
			Item = item;
		}
	}
}
