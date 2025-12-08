using Game.Models;
using Game.Terrain;

using System;
using System.Text;


namespace Game.Narration.WorldDescribers
{
	public class ExitDescriber
	{

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

		public string GetExitDescription(ExitInfo info)
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
	}
}
