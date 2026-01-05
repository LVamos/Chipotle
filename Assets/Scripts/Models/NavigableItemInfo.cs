using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Models
{
	public class NavigableItemInfo:NavigableObjectInfo
	{
		public Item Item;

		public NavigableItemInfo(float distance, Item item, float angle, float observerStepLength,Character observer)
			:base(distance, angle, observerStepLength, observer)
		{
			Item = item;
		}
	}
}
