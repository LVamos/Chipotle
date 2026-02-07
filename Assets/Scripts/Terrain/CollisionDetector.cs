using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Models;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Terrain
{
	public class CollisionDetector
	{
		/// <summary>
		/// Detects acoustic obstacles between the player and the specified map element.
		/// </summary>
		/// <param name="area">The map element to be checked</param>
		/// <returns>The corresponding obstacle type</returns>
		public ObstacleType DetectOcclusion(MapElement emittingObject, bool ignoreSubtleObjects = true, bool ignoreItems = true)
		{
			Vector2 playerCenter = World.Player.Center;
			Vector2 closestPoint = emittingObject.Area.Value.GetClosestPoint(playerCenter);

			Rectangle ray = Rectangle.FromCenter(closestPoint, 0.1f, 0.1f, false);
			float distance = World.GetDistance(ray.Center, playerCenter);
			Vector2 direction = (playerCenter - ray.Center).normalized;

			List<MapElement> ignored = new() { emittingObject, World.Player };

			TrackCollisionParams parameters = new(
				direction,
				distance,
				ignored,
				ray,
				false,
				false,
				false,
				ignoreItems
			);

			Collisions result = DetectOnTrack(parameters);
			if (result.Obstacles == null) return ObstacleType.None;

			return ClassifyObstacle(result.Obstacles);
		}

		private ObstacleType ClassifyObstacle(IEnumerable<object> obstacles)
		{
			if (obstacles.Any(o => o is Item i && i.Type == "zeď")) return ObstacleType.Wall;
			if (obstacles.Any(o => o is Door d && d.State is PassageState.Closed or PassageState.Locked)) return ObstacleType.ClosedDoor;
			if (obstacles.Any(o => o is Character)) return ObstacleType.ItemOrCharacter;
			return ObstacleType.None;
		}

		/// <summary>
		/// Detects collisions on the given track area in the specified direction for the given length.
		/// </summary>
		/// <param name="area">The rectangle representing the track area.</param>
		/// <param name="direction">The direction in which to detect collisions.</param>
		/// <param name="length">The length for which to detect collisions in meters</param>
		/// <returns>A list of MapElements representing the obstacles detected on the track or null</returns>
		/// <remarks>Divides the track to little segments and in every position checks all objects, closed passages and characters in intersecting zones for collision. The search ends at the position where collisions were detected.</remarks>
		public Collisions DetectOnTrack(TrackCollisionParams parameters)
		{
			int steps = Mathf.CeilToInt(parameters.Length / CollisionDetectionResolution) + 1;
			Rectangle capsule = parameters.Area;

			for (int i = 0; i < steps; i++)
			{
				if (i > 0)
				{
					Vector2 offset = parameters.Direction * CollisionDetectionResolution * i;
					capsule.Resize(parameters.Area.Center + offset, parameters.Area.Width, parameters.Area.Height);
				}

				Collisions result = Detect(parameters.WithArea(capsule));
				if (result.OutOfMap
					|| HasBlockingObstacle(result.Obstacles))
					return result;
			}

			return new(null, false);
		}

		private static bool HasBlockingObstacle(IEnumerable<object> obstacles)
		{
			return obstacles != null && obstacles.Any(o =>
		(o is Item i && !i.Passable) ||
		(o is Door d && !d.Open));
		}

		private const float _subtleObjectSizeThreshold = 0.04f;

		public const float CollisionDetectionResolution = .1f;

		/// <summary>
		/// Detects collisions between the specified map element and other map elements.
		/// </summary>
		/// <param name="ignoredElements">The element to be moved.</param>
		/// <param name="area">The map element to detect collisions for.</param>
		/// <returns>List of MapElements or null</returns>
		public Collisions Detect(CollisionParams parameters)
		{
			List<object> obstacles = new();
			List<Zone> zones = parameters.Area.GetZones().ToList();

			// Detect inaccesible terrain
			if (parameters.Terrain)
			{
				var inaccessibleTiles = CheckTerrain(parameters);
				if (inaccessibleTiles.Count > 0)
				{
					obstacles.AddRange(inaccessibleTiles.Cast<object>());
					if (parameters.FirstHit)
						return new(obstacles, false);
				}
			}

			// Check all objects, passages, and characters in zones
			foreach (Zone z in zones)
			{
				if (!z.IsWalkable(parameters.Area))
				{
					ProcessZoneCollection(z.Items, parameters, obstacles, parameters.IgnoreSmall, parameters.IgnoreItems);
					if (Enough()) return new(obstacles, false);
				}

				// Always check these regardless of walkability
				var zoneCollections = new List<IEnumerable<MapElement>>()
		{
			z.Characters,
			z.MovableItems,
			z.GetClosedDoors()
		};

				foreach (var collection in zoneCollections)
				{
					ProcessZoneCollection(collection, parameters, obstacles);
					if (Enough()) return new(obstacles, false);
				}
			}

			bool outOfMap = parameters.Area.IsOutOfMap();
			var finalObstacles = obstacles.Count > 0 ? obstacles.Distinct().ToList() : null;
			return new(finalObstacles, outOfMap);

			// Local function to keep FirstHit logic
			bool Enough() => parameters.FirstHit && obstacles.Count > 0;
		}

		private void ProcessZoneCollection(
			IEnumerable<MapElement> elements,
			CollisionParams parameters,
			List<object> obstacles,
			bool ignoreSmall = false,
			bool ignoreItems = false)
		{
			CheckElements(elements, parameters, obstacles, ignoreSmall, ignoreItems);
		}

		private static List<TileInfo> CheckTerrain(CollisionParams parameters)
		{
			List<TileInfo> allTiles = parameters.Area.GetTiles(TileMap.TileSize);
			List<TileInfo> inaccessibleTiles = allTiles
				.Where(t => !t.Tile.Walkable)
				.Distinct().ToList();
			return inaccessibleTiles;
		}

		private void CheckElements(
	IEnumerable<MapElement> elements,
	CollisionParams parameters,
	List<object> obstacles,
	bool ignoreSmall = false,
	bool ignoreItems = false)
		{
			bool IsIgnored(MapElement e) => parameters.Ignored?.Contains(e) ?? false;

			var newObstacles = elements
				.Where(e => e.Area != null && !IsIgnored(e))
				.Where(e => e.Area.Value.IntersectsStrict(parameters.Area)
						 || e.Area.Value.Contains(parameters.Area)
						 || parameters.Area.Contains(e.Area.Value))
				.Where(e => !ignoreSmall || e.Area.Value.Size > _subtleObjectSizeThreshold)
				.Where(e => !ignoreItems || (e is Item i && i.Type == "zeď"))
				.ToList();

			if (newObstacles.Count > 0)
				obstacles.AddRange(newObstacles);
		}

	}
}
