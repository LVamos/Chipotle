using Game.Controls.DualSense;
using Game.Models;
using Game.Terrain;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Game.Narration.WorldDescribers
{
	public class ExitDescriber
	{
		public List<List<string>> GetStructuredDescriptions(List<ExitInfo> exits)
		{
			List<List<string>> descriptions =
				exits
				.Select(GetStructuredDescription)
				.ToList();
			return descriptions;
					}

		/// <summary>
		/// Generates a text representation of the specified distance in Czech.
		/// </summary>
		/// <param name="distance">The distance in meters to be described</param>
		/// <returns>The text representation of the specified distance</returns>
		private string GetDistanceDescription(ExitInfo info)
		{
			if (info.Distance <= info.StepLength)
				return string.Empty;

			// Round the distance so that its value corresponds to a multiple of 0.5.
			int steps = (int)Math.Round(info.Distance / info.StepLength);

			// Compose output
			if (steps == 1)
				return "jeden krok";
			if (steps is > 1 and < 5)
				return $"{steps} kroky";
			return $"{steps} kroků";
		}

		public string GetDescription(ExitInfo info)
		{
			string distanceDescription = GetDistanceDescription(info);
			string type = info.Exit.TypeDescription;

			string to = "", to1 = "", to2 = "";
			if (info.Exit is Door door && !door.OpenedPreviously)
			{
				to1 = door.Name.Friendly;
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
			StringBuilder builder = new();
			builder.Append(type)
				.Append(to1)
				.Append(to2)
				.Append(distanceDescription)
				.Append(angleDescription);
			if (Settings.SayInnerZoneNames)
				builder.Append($" {info.TargetZone.Name.Indexed}");
			return builder.ToString();
		}

		public List<string> GetStructuredDescription(ExitInfo info)
		{
			// If it's a door and hasn't been opened return one line record for simple searching.
			if (info.Exit is Door tempDoor && !tempDoor.OpenedPreviously)
				return new List<string>() { GetDescription(info) };

			string type = info.Exit.TypeDescription;
			string distanceDescription = GetDistanceDescription(info);

			string to = "", to1 = "", to2 = "";
			if (info.Exit is Door door && !door.OpenedPreviously)
			{
				to1 = door.Name.Friendly;
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
			List<string> record = new();
			if (!string.IsNullOrEmpty(type))
				record.Add(type);
			record.Add(to1);
			record.Add(to2);
			record.Add(distanceDescription);
			record.Add(angleDescription);
			if (Settings.SayInnerZoneNames)
				record.Add($" {info.TargetZone.Name.Indexed}");

			return record;
		}
	}
}
