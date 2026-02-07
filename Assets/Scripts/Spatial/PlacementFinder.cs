using Game;
using Game.Models;
using Game.Terrain;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Assets.Scripts.Spatial
{
	public class PlacementFinder
	{
		public const float ValidplacementsResolution = 1;

		public IEnumerable<Vector2> GetFreePlacements(List<MapElement> ignoredElements, Rectangle areaToAvoid, float height, float width)
		{
			List<Vector2> placements = new();

			HashSet<Vector2> points = areaToAvoid.GetPoints(ValidplacementsResolution);
			foreach (Vector2 point in points)
			{
				CollisionParams parameters = new(
					ignoredElements,
					Rectangle.FromCenter(point, height, width));
				Collisions collisions = World.Collisions.Detect(parameters);
				if (collisions is { Obstacles: null, OutOfMap: false })
					placements.Add(point);
			}
			return placements;
		}

		public IEnumerable<Vector2> GetFreePlacementsNear(List<MapElement> ignoredElements, Rectangle areaToAvoid, float height, float width, float minDistance, float maxDistance, bool sameZone = true)
		{
			Rectangle maxArea = areaToAvoid;
			maxArea.Extend(maxDistance);

			IEnumerable<Vector2> candidatePoints = GetFreePlacements(ignoredElements, maxArea, height, width);

			Zone zone = World.GetZone(areaToAvoid.Center);
			IEnumerable<Vector2> filteredPoints =
				from point in candidatePoints
				let inSameZone = World.GetZone(point) == zone
				let tempArea = Rectangle.FromCenter(point, height, width)
				let distance = areaToAvoid.GetDistanceFrom(tempArea)
				let allowedDistance = distance >= minDistance && distance <= maxDistance
				where inSameZone && allowedDistance
				orderby distance
				select point
				;
			return filteredPoints;
		}


	}
}
