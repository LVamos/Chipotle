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
		public bool IntersectsWithCharacter;
		public Item Item;
		public int ClockHour;

		public NavigableItemInfo(float distance, Item item, float angle, float observerStepLength,Character observer, bool intersectsWithCharacter, int clockHour)
			:base(distance, angle, observerStepLength, observer)
		{
			Item = item;
			IntersectsWithCharacter = intersectsWithCharacter;
			ClockHour = clockHour;
		}
	}
}
