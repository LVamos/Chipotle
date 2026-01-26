using System.Collections.Generic;

namespace Game.Terrain
{
	public class CollisionParams
	{
		public CollisionParams(
			List<MapElement> ignored,
			Rectangle area,
			bool justFirstObstacle = false,
			bool checkTerrain = true,
			bool ignoreSubtleObjects = false,
			bool ignoreItems = false)
		{
			Ignored = ignored;
			Area = area;
			FirstHit = justFirstObstacle;
			Terrain = checkTerrain;
			IgnoreSmall = ignoreSubtleObjects;
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
