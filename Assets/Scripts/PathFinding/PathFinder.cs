using Game.Entities.Characters;
using Game.Models;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;

namespace Game.PathFinding
{
	/// <summary>
	/// Implementation of A* algorithm with PriorityQueue + Dictionary for open set,
	/// plus a bounding box to limit the search area.
	/// </summary>
	[Serializable]
	public static class PathFinder
	{
		private const float _distanceToleration = .5f;
		private const float _boundingBoxMargin = 5;

		static BoundingBox _box;

		private static List<Rectangle> _zones;

		private static HashSet<Vector2> _nonpassables;

		/// <summary>
		/// Najde cestu.
		/// </summary>
		public static Queue<Vector2> FindPath(PathfindingParams parameters)
		{
			// validate the sameZone parameter.
			Zone startZone = World.GetZone(parameters.Start);
			Zone targetZone = World.GetZone(parameters.Goal);
			if (parameters.SameZone && startZone != targetZone)
				return null;

			// Calculate bounding box to prevent the algorithm from searching the whole map.
			_box = BoundingBox.Create(parameters.Start, parameters.Goal, _boundingBoxMargin);
			PathFindingNode startNode = new(parameters.Start, cost: 0, parent: null);
			startNode.ComputeDistance(parameters.Goal);

			PriorityQueue<PathFindingNode, float> openQueue = new(); // For the cheapest node (Price)

			Dictionary<Vector2, PathFindingNode> open = new(); // Dictionary to a quick finding, if the node is already there
			HashSet<Vector2> closed = new(); // set of closed nodes

			// I put the start
			openQueue.Enqueue(startNode, startNode.Price);
			open[parameters.Start] = startNode;

			while (openQueue.Count > 0)
			{
				PathFindingNode current = openQueue.Dequeue();

				open.Remove(current.Coords);

				// If Current is close to the target, I am returning the path
				if (Vector2.Distance(current.Coords, parameters.Goal) <= _distanceToleration)
					return GetPath(current, parameters.ThroughStart, parameters.ThroughGoal);

				closed.Add(current.Coords);

				List<PathFindingNode> neighbours = GetNeighbours(current, parameters.Start, parameters.Goal, parameters.SameZone, parameters.ThroughStart, parameters.ThroughGoal).ToList();
				foreach (PathFindingNode neighbour in neighbours)
				{

					// Is not walkable or is already closed => skip
					if (closed.Contains(neighbour.Coords)
						|| !IsWalkable(neighbour, parameters)
						)
					{
						closed.Add(neighbour.Coords);
						continue;
					}

					// I'll try if it's a new node or there is already there
					float newCost = current.Cost + 1;
					if (!open.TryGetValue(neighbour.Coords, out PathFindingNode existing))
					{
						neighbour.Cost = newCost;
						neighbour.Parent = current;
						neighbour.ComputeDistance(parameters.Goal);

						openQueue.Enqueue(neighbour, neighbour.Price);
						open[neighbour.Coords] = neighbour;
					}
					else
					{
						// update if there is now a cheaper way
						if (newCost < existing.Cost)
						{
							existing.Cost = newCost;
							existing.Parent = current;
							existing.ComputeDistance(parameters.Goal);

							// re -put in PQ to update the priority
							openQueue.Enqueue(existing, existing.Price);
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Metoda, která vytváří sousedy (4-směrné)
		/// </summary>
		private static IEnumerable<PathFindingNode> GetNeighbours(
			PathFindingNode parent,
			Vector2 start,
			Vector2 goal,
			bool sameZone,
			bool throughStart,
			bool throughGoal
		)
		{
			List<TileInfo> tiles = World.Map.GetNeighbours4(parent.Coords).ToList();

			foreach (Models.TileInfo tileInfo in tiles)
			{
				// => bounding box check tady (nebo klidně v IsWalkable):
				Vector2 pos = tileInfo.Position;
				if (pos.x < _box.Min.x || pos.x > _box.Max.x || pos.y < _box.Min.y || pos.y > _box.Max.y)
				{
					// je to mimo bounding box => ignoruju
					continue;
				}

				PathFindingNode n = new(pos, cost: 0, parent: null);
				yield return n;
			}
		}

		/// <summary>
		/// Zkontroluje, zda je tile průchozí atd.
		/// </summary>
		private static bool IsWalkable(PathFindingNode node, PathfindingParams parameters)
		{
			Rectangle tempAre = parameters.IgnoredCharacter.Area.Value;
			Rectangle area = Rectangle.FromCenter(node.Coords, tempAre.Height, tempAre.Width);
			Zone zone = World.Map[area.Center].Zone;
			bool noStaticObjects = zone.IsWalkable(area);
			bool walkableTerrain = area.GetTiles(TileMap.TileSize)
				.All(t => t != null && t.Tile.Walkable);
			Tile tile = World.Map[node.Coords];
			bool noCharacters = !zone.Characters.Any(c => c != parameters.IgnoredCharacter && c.Area.Value.Contains(node.Coords));
			//test
			noCharacters = true;
			bool walkable = walkableTerrain && noStaticObjects && noCharacters;
			if (!walkable)
				return false;

			// If the node lays on start or goal, exclude it if not allowed.
			if ((!parameters.ThroughStart && node.Coords == parameters.Start) ||
				(!parameters.ThroughGoal && node.Coords == parameters.Goal))
				return false;

			return true;
		}

		/// <summary>
		/// Rekonstruuje cestu od nalezeného konce ke startu.
		/// </summary>
		private static Queue<Vector2> GetPath(PathFindingNode lastPoint, bool throughStart, bool throughGoal)
		{
			Stack<Vector2> stack = new();
			for (PathFindingNode step = lastPoint; step != null; step = step.Parent)
				stack.Push(step.Coords);

			Queue<Vector2> path = new(stack);

			int count = path.Count;
			if (
				count == 0
				|| ((throughStart ^ throughGoal) && count == 1)
				|| (throughStart && throughGoal && count == 2)
			)
				return null;

			if (!throughStart)
				path.Dequeue();

			return path;
		}
	}
}
