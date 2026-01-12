using Game;
using Game.Models;
using Game.Narration.WorldDescribers;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.Profiling;

namespace Assets.Scripts.Narration.WorldDescribers
{
	public class NavigableItemDescriber:NavigableObjectDescriber
	{
		public override List<string> GetDescriptions(List<NavigableObjectInfo> records)
		{
List<NavigableItemInfo> info = 				records
				.Cast<NavigableItemInfo>()
				.ToList();
			List<string> descriptions =
				info
				.Select(GetDescription)
				.ToList();
			return descriptions;
		}

		public override List<List<string>> GetStructuredDescriptions(List<NavigableObjectInfo> items)
		{
			List<List<string>> descriptions =
				items
				.Select(GetStructuredDescription)
				.ToList();
			return descriptions;
		}

		public override List<string> GetStructuredDescription(NavigableObjectInfo record)
		{
			NavigableItemInfo info = record as NavigableItemInfo;
			string distanceDescription = GetDistanceDescription(info);
			string angleDescription;
			if (info.IntersectsWithCharacter)
				angleDescription = "tady";
			else angleDescription = Angle.GetClockDirection(info.Angle);

			string name = info.Item.Name.Friendly;
			if (Settings.SayInnerItemNames)
				name += " " + info.Item.Name.Indexed;

			// Join it all
			List<string> result = new(){name,distanceDescription,angleDescription};
			return result;
		}

	}
}
