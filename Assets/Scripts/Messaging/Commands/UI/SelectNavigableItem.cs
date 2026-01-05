using Assets.Scripts.Models;

using Game.Models;

using System.Collections.Generic;

namespace Game.Messaging.Commands.UI
{
	public class SelectNavigableItem : Message
	{
		public readonly List<NavigableItemInfo> Items;

		public SelectNavigableItem(object sender, List<NavigableItemInfo> items) : base(sender)
		{
			Items = items;
		}
	}
}
