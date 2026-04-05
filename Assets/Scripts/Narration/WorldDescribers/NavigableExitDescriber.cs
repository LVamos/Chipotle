using Game.Models;
using Game.Terrain;

using System.Collections.Generic;
using System.Linq;


namespace Game.Narration.WorldDescribers
{
	public class NavigableExitDescriber : NavigableObjectDescriber
	{
		public override List<string> GetDescriptions(List<NavigableObjectInfo> records)
		{
			List<NavigableExitInfo> info = records
							.Cast<NavigableExitInfo>()
							.ToList();
			List<string> descriptions =
				info
				.Select(GetDescription)
				.ToList();
			return descriptions;
		}


		public override List<List<string>> GetStructuredDescriptions(List<NavigableObjectInfo> records)
		{
			List<NavigableExitInfo> exits =
				records
					.Cast<NavigableExitInfo>()
	.ToList();

			List<List<string>> descriptions =
				exits
				.Select(GetStructuredDescription)
				.ToList();
			return descriptions;
		}

		public override List<string> GetStructuredDescription(NavigableObjectInfo record)
		{
			NavigableExitInfo info = record as NavigableExitInfo;

			string type = info.Exit.TypeDescription;
			string distanceDescription = GetDistanceDescription(info);

			string to = "", to1 = "", to2 = "";

			// Solve destination announcement
			Passage exit = info.Exit;
			Zone targetZone = info.TargetZone;
			bool visited = info.Observer.VisitedZones.Contains(targetZone);
			if (exit is Door && !visited)
			{
				to1 = info.Exit.Name.Friendly;
				type = "";
			}
			else
			{
				to = info.TargetZone.To;
				int index = to.IndexOf(' ');
				to1 = to.Substring(0, index);
				to2 = to.Substring(index + 1);
			}

			string angleDescription = Angle.GetClockDirection(info.Angle);

			// Join it all

			List<string> result = new();
			if (!string.IsNullOrEmpty(type))
				result.Add(type);
			result.Add(to1);
			result.Add(to2);
			result.Add(distanceDescription);
			result.Add(angleDescription);
			if (Settings.SayInnerZoneNames)
				result.Add($" {info.TargetZone.Name.Inner}");

			// If it's a door and hasn't been opened return one line record for simple searching.
			if (info.Exit is Door tempDoor && !tempDoor.OpenedPreviously)
				return new() { string.Join(' ', result) };
			return result;
		}
	}
}
