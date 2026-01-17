using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			CollisionsModel result = DetectCollisionsOnTrack(ignored, ray, direction, distance, false, false, false, ignoreItems);
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
		public CollisionsModel DetectCollisionsOnTrack(List<MapElement> ignoredElements, Rectangle initialPosition, Vector2 direction, float length, bool justFirstObstacle = false, bool checkTerrain = true, bool ignoreSubtleObjects = false, bool ignoreItems = false)
		{
			int steps = Mathf.CeilToInt((length) / CollisionDetectionResolution);
			steps++; // include the initial position

			CollisionsModel result = DetectCollisions(ignoredElements, initialPosition, justFirstObstacle, checkTerrain, ignoreSubtleObjects, ignoreItems);
			if (result.OutOfMap)
				return result;
			if (!result.Obstacles.IsNullOrEmpty())
				return result;

			Rectangle capsule;
			for (int i = 0; i < steps; i++)
			{
				Vector2 offset = direction * CollisionDetectionResolution * i;
				Vector2 newCenter = new Vector2(initialPosition.Center.x + offset.x, initialPosition.Center.y + offset.y);
				capsule = Rectangle.FromCenter(newCenter, initialPosition.Height, initialPosition.Width, false);

				result = DetectCollisions(ignoredElements, capsule, justFirstObstacle, checkTerrain, ignoreSubtleObjects, ignoreItems);
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
		public CollisionsModel DetectCollisions(List<MapElement> ignoredElements, Rectangle area, bool justFirstObstacle = false, bool checkTerrain = true, bool ignoreSubtleObjects = false, bool ignoreItems = false)
		{
			List<object> obstacles = new();
			List<Zone> zones = area.GetZones().ToList();

			// Detect inaccesible terrain.
			if (checkTerrain)
			{
				List<TileInfo> allTiles = area.GetTiles(TileMap.TileSize);
				List<TileInfo> inaccessibleTiles = allTiles
					.Where(t => !t.Tile.Walkable)
					.Distinct().ToList();

				if (inaccessibleTiles.Any())
				{
					obstacles.AddRange(inaccessibleTiles.Cast<object>());
					if (justFirstObstacle)
						return new(obstacles, false);
				}
			}

			// Check all objects, passages and characters in zones the given element intersects. Skip the given element.
			foreach (Zone z in zones)
			{
				bool probablyWalkable = z.IsWalkable(area);
				if (!probablyWalkable)
				{
					// make sure there is an item. If not, allow further collision detection.
					Detect(z.Items, ignoreSubtleObjects, ignoreItems);
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

			bool outOfMap = area.IsOutOfMap();
			if (!obstacles.Any())
				obstacles = null;
			else obstacles = obstacles.Distinct().ToList();
			return new(obstacles, outOfMap);

			void Detect(IEnumerable<MapElement> elements, bool ignoreSubtleObjects = false, bool ignoreItems = false)
			{
				bool IsIgnoredElement(MapElement element) => ignoredElements != null && ignoredElements.Contains(element);

				List<MapElement> newObstacles = elements
					.Where(element => element.Area != null
					&& !IsIgnoredElement(element))
					.Where(e => e.Area.Value.IntersectsStrict(area) || e.Area.Value.Contains(area) || area.Contains(e.Area.Value))
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

			bool Enough() => justFirstObstacle && obstacles.Count > 0;
		}

	}
}
