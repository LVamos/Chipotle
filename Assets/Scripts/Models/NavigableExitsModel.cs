using Game.Terrain;

using System.Collections.Generic;

namespace Game.Models
{
	/// <summary>
	/// Model representing navigable exits with descriptions and passages.
	/// </summary>
	public class NavigableExitsModel
	{
		public List<List<string>> Descriptions { get; set; }
		public Passage[] Exits { get; set; }

		public NavigableExitsModel(List<List<string>> descriptions, Passage[] exits)
		{
			Descriptions = descriptions;
			Exits = exits;
		}
	}
}
