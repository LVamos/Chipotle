using System.Collections.Generic;

using UnityEngine;

namespace Game.Terrain
{
	public class TrackCollisionParams : CollisionParams
	{
		public TrackCollisionParams WithArea(Rectangle newArea)
		{
			return new
				(
				Direction,
				Length,
				Ignored,
				newArea,
				FirstHit,
				Terrain,
				IgnoreSmall,
				IgnoreItems
				);
		}

		public float Length;
		public Vector2 Direction;

		public TrackCollisionParams(
			Vector2 direction,
			float length,
			List<MapElement> ignoredElements,
			Rectangle area,
			bool justFirstObstacle = false,
			bool checkTerrain = true,
			bool ignoreSubtleObjects = false,
			bool ignoreItems = false
			)
			: base(ignoredElements, area, justFirstObstacle, checkTerrain, ignoreSubtleObjects, ignoreItems)
		{
			Length = length;
			Direction = direction;
		}

	}
}
