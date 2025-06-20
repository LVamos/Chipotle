using Game.Terrain;

using System.Collections.Generic;

namespace Game.Models
{
	/// <summary>
	/// Model representing navigable exits with descriptions and passages.
	/// </summary>
	public class NavigableExitsModel
	{
		public readonly List<Zone> TargetZones;
		public readonly List<List<string>> Descriptions;
		public List<Passage> Exits { get; set; }

		public NavigableExitsModel(List<List<string>> descriptions, List<Passage> exits, List<Zone> targetZones)
		{
			Descriptions = descriptions;
			Exits = exits;
			TargetZones = targetZones;
		}
	}
}
