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
	public class NavigableCharacterDescriber:NavigableObjectDescriber
	{
		public override List<string> GetDescriptions(List<NavigableObjectInfo> records)
		{
List<NavigableCharacterInfo> info = 				records
				.Cast<NavigableCharacterInfo>()
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
			NavigableCharacterInfo info = record as NavigableCharacterInfo;
			string distanceDescription = GetDistanceDescription(info);
			string angleDescription = Angle.GetClockDirection(info.Angle);
			string name = info.Character.Name.Friendly;
			if (Settings.SayInnerItemNames)
				name += " " + info.Character.Name.Indexed;

			// Join it all
			return new(){name,distanceDescription,angleDescription};
		}

	}
}
