using System.Collections.Generic;

using UnityEngine;

namespace Game.Terrain
{
	public class TrackCollisionParams : CollisionParams
	{
		public TrackStopMode Mode;

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
			List<MapElement> ignored,
			Rectangle area,
			bool firstHit = false,
			bool terrain = true,
			bool ignoreSmall = false,
			bool ignoreItems = false,
			TrackStopMode mode = TrackStopMode.Blocking
			)
			: base(ignored, area, firstHit, terrain, ignoreSmall, ignoreItems)
		{
			Length = length;
			Direction = direction;
			Mode = mode;
		}

	}
}
