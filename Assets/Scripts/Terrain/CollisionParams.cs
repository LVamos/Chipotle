using System.Collections.Generic;

namespace Game.Terrain
{
	public class CollisionParams
	{
		public CollisionParams(
			List<MapElement> ignored,
			Rectangle area,
			bool firstHit = false,
			bool terrain = true,
			bool ignoreSmall = false,
			bool ignoreItems = false)
		{
			Ignored = ignored;
			Area = area;
			FirstHit = firstHit;
			Terrain = terrain;
			IgnoreSmall = ignoreSmall;
			IgnoreItems = ignoreItems;
		}

		public List<MapElement> Ignored;
		public Rectangle Area;
		public bool FirstHit;
		public bool Terrain;
		public bool IgnoreSmall;
		public bool IgnoreItems;
	}
}
