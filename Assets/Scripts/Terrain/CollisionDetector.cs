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
		public ObstacleType DetectOcclusion(MapElement emmittingObject, bool ignoreSubtleObjects = true, bool ignoreItems = true)
		{
			Vector2 playerCenter = World.Player.Center;
			Vector2 closestPoint = emmittingObject.Area.Value.GetClosestPoint(playerCenter);

			Rectangle ray = Rectangle.FromCenter(closestPoint, .1f, .1f, false);
			float distance = World.GetDistance(ray.Center, playerCenter);
			Vector2 direction = (playerCenter - ray.Center).normalized;
			List<MapElement> ignored = new() { emmittingObject, World.Player };
			TrackCollisionParams parameters = new
				(
				direction,
				distance,
				ignored,
				ray,
				false,
				false,
				false,
				ignoreItems
				);
			CollisionsModel result = DetectOnTrack(parameters);
			if (result.Obstacles == null)
				return ObstacleType.None;

			if (result.Obstacles.Any(o => o is Item i && i.Type == "zeď"))
				return ObstacleType.Wall;

			if (result.Obstacles.Any(o => o is Door d && d.State is PassageState.Closed or PassageState.Locked))
				return ObstacleType.ClosedDoor;

			if (result.Obstacles.Any(o => o is Character))
				return ObstacleType.ItemOrCharacter;
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
		public CollisionsModel DetectOnTrack(TrackCollisionParams parameters)
		{
			int steps = Mathf.CeilToInt((parameters.Length) / CollisionDetectionResolution);
			steps++; // include the initial position
			CollisionsModel result = Detect(parameters);
			if (result.OutOfMap)
				return result;
			if (!result.Obstacles.IsNullOrEmpty())
				return result;

			for (int i = 0; i < steps; i++)
			{
				Vector2 offset = parameters.Direction * CollisionDetectionResolution * i;
				Vector2 newCenter = new Vector2(parameters.Area.Center.x + offset.x, parameters.Area.Center.y + offset.y);
				Rectangle capsule = Rectangle.FromCenter(newCenter, parameters.Area.Height, parameters.Area.Width, false);
				result = Detect(parameters.WithArea(capsule));
				if (result.OutOfMap)
					return result;
				if (!result.Obstacles.IsNullOrEmpty())
					return result;
			}

			return new(null, false);
		}

		private const float _subtleObjectSizeThreshold = 0.04f;

		public const float CollisionDetectionResolution = .1f;

		/// <summary>
		/// Detects collisions between the specified map element and other map elements.
		/// </summary>
		/// <param name="ignoredElements">The element to be moved.</param>
		/// <param name="area">The map element to detect collisions for.</param>
		/// <returns>List of MapElements or null</returns>
		public CollisionsModel Detect(CollisionParams parameters)
		{
			List<object> obstacles = new();
			List<Zone> zones = parameters.Area.GetZones().ToList();

			// Detect inaccesible terrain.
			if (parameters.Terrain)
			{
				List<TileInfo> allTiles = parameters.Area.GetTiles(TileMap.TileSize);
				List<TileInfo> inaccessibleTiles = allTiles
					.Where(t => !t.Tile.Walkable)
					.Distinct().ToList();

				if (inaccessibleTiles.Any())
				{
					obstacles.AddRange(inaccessibleTiles.Cast<object>());
					if (parameters.FirstHit)
						return new(obstacles, false);
				}
			}

			// Check all objects, passages and characters in zones the given element intersects. Skip the given element.
			foreach (Zone z in zones)
			{
				bool probablyWalkable = z.IsWalkable(parameters.Area);
				if (!probablyWalkable)
				{
					// make sure there is an item. If not, allow further collision detection.
					Detect(z.Items, parameters.IgnoreSmall, parameters.IgnoreItems);
					if (Enough())
						return new(obstacles, false);
				}

				Detect(z.Characters);
				if (Enough())
					return new(obstacles, false);

				Detect(z.MovableItems);
				if (Enough())
					return new(obstacles, false);

				Detect(z.GetClosedDoors());
				if (Enough())
					return new(obstacles, false);
			}

			bool outOfMap = parameters.Area.IsOutOfMap();
			if (!obstacles.Any())
				obstacles = null;
			else obstacles = obstacles.Distinct().ToList();
			return new(obstacles, outOfMap);

			void Detect(IEnumerable<MapElement> elements, bool ignoreSubtleObjects = false, bool ignoreItems = false)
			{
				bool IsIgnoredElement(MapElement element) => parameters.Ignored != null && parameters.Ignored.Contains(element);

				List<MapElement> newObstacles = elements
					.Where(element => element.Area != null
					&& !IsIgnoredElement(element))
					.Where(e => e.Area.Value.IntersectsStrict(parameters.Area) || e.Area.Value.Contains(parameters.Area) || parameters.Area.Contains(e.Area.Value))
.ToList();
				if (ignoreSubtleObjects)
					newObstacles = newObstacles
					.Where(o => o.Area.Value.Size > _subtleObjectSizeThreshold)
					.ToList();

				if (ignoreItems)
					newObstacles = newObstacles
					.Where(o => o is Item i && i.Type == "zeď")
					.ToList();

				if (newObstacles.Any())
					obstacles.AddRange(newObstacles);
			}

			bool Enough() => parameters.FirstHit && obstacles.Count > 0;
		}

	}
}
