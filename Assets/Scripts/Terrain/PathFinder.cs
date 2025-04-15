using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Terrain
{
	/// <summary>
	/// Implementation of A* algorithm with PriorityQueue + Dictionary for open set,
	/// plus a bounding box to limit the search area.
	/// </summary>
	[Serializable]
	public class PathFinder
	{
		private const float _distanceToleration = .5f;
		private const float _boundingBoxMargin = 10f;

		// Lokální proměnné, do kterých uložíme bounding box při hledání
		private float _minX, _maxX, _minY, _maxY;

		private List<Rectangle> _zones;

		private HashSet<Vector2> _nonpassables;

		/// <summary>
		/// Najde cestu.
		/// </summary>
		public Queue<Vector2> FindPath(
			Vector2 start,
			Vector2 goal,
			bool sameZone,
			bool throughStart,
			bool throughGoal,
			float characterHeight,
			float characterWidth
		)
		{
			// validate the sameZone parameter.
			Zone startZone = World.Map[start].Zone;
			Zone targetZone = World.Map[goal].Zone;
			if (sameZone && startZone != targetZone)
				return null;

			// Calculate bounding box to prevent the algorithm from searching the whole map.
			float margin = _boundingBoxMargin;
			_minX = Mathf.Min(start.x, goal.x) - margin;
			_maxX = Mathf.Max(start.x, goal.x) + margin;
			_minY = Mathf.Min(start.y, goal.y) - margin;
			_maxY = Mathf.Max(start.y, goal.y) + margin;

			PathFindingNode startNode = new(start, cost: 0, parent: null);
			startNode.ComputeDistance(goal);

			PriorityQueue<PathFindingNode, float> openQueue = new(); // For the cheapest node (Price)

			Dictionary<Vector2, PathFindingNode> open = new(); // Dictionary to a quick finding, if the node is already there
			HashSet<Vector2> closed = new(); // set of closed nodes

			// I put the start
			openQueue.Enqueue(startNode, startNode.Price);
			open[start] = startNode;

			while (openQueue.Count > 0)
			{
				PathFindingNode current = openQueue.Dequeue();

				open.Remove(current.Coords);

				// If Current is close to the target, I am returning the path
				if (Vector2.Distance(current.Coords, goal) <= _distanceToleration)
					return GetPath(current, throughStart, throughGoal);

				closed.Add(current.Coords);

				IEnumerable<PathFindingNode> neighbours = GetNeighbours(current, start, goal, sameZone, throughStart, throughGoal);
				foreach (PathFindingNode neighbour in neighbours)
				{

					// Is not walkable or is already closed => skip
					if (closed.Contains(neighbour.Coords)
						|| !IsWalkable(neighbour, start, goal, sameZone, throughStart, throughGoal, characterHeight, characterWidth)
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
						neighbour.ComputeDistance(goal);

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
							existing.ComputeDistance(goal);

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
		private IEnumerable<PathFindingNode> GetNeighbours(
			PathFindingNode parent,
			Vector2 start,
			Vector2 goal,
			bool sameZone,
			bool throughStart,
			bool throughGoal
		)
		{
			// Vezmu sousedy 4-směrné
			IEnumerable<Models.TileInfo> tiles = World.Map.GetNeighbours4(parent.Coords);

			foreach (Models.TileInfo tileInfo in tiles)
			{
				// => bounding box check tady (nebo klidně v IsWalkable):
				Vector2 pos = tileInfo.Position;
				if (pos.x < _minX || pos.x > _maxX || pos.y < _minY || pos.y > _maxY)
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
		private bool IsWalkable(
			PathFindingNode node,
			Vector2 start,
			Vector2 goal,
			bool sameZone,
			bool throughStart,
			bool throughGoal,
			float characterHeight,
			float characterWidth
		)
		{
			//test
			Rectangle area = Rectangle.FromCenter(node.Coords, characterHeight, characterWidth);
			Zone zone = World.Map[area.Center].Zone;
			bool noStaticObjects = zone.IsWalkable(area);
			bool walkableTerrain = area.GetTiles(World.Map.TileSize)
				.All(t => t != null && t.Tile.Walkable);
			Tile tile = World.Map[node.Coords];
			bool walkable = walkableTerrain && noStaticObjects && !zone.Characters.Any(c => c.Area.Value.Contains(node.Coords));
			if (!walkable)
				return false;

			// If the node lays on start or goal, exclude it if not allowed.
			if ((!throughStart && node.Coords == start) ||
				(!throughGoal && node.Coords == goal))
				return false;

			return true;
		}

		/// <summary>
		/// Rekonstruuje cestu od nalezeného konce ke startu.
		/// </summary>
		private Queue<Vector2> GetPath(PathFindingNode lastPoint, bool throughStart, bool throughGoal)
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
