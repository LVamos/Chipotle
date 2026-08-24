using Game.Entities.Characters;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

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
		private static Dictionary<Zone, HashSet<Vector2>> _characterPoints;

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

			CountCharacterPoints(parameters.IgnoredCharacter);

			PriorityQueue<PathFindingNode, float> openQueue = new(); // For the cheapest node (Price)

			Dictionary<Vector2, PathFindingNode> open = new(); // Dictionary to a quick finding, if the node is already there
			HashSet<Vector2> closed = new(); // set of closed nodes

			// I put the start
			openQueue.Enqueue(startNode, startNode.Price);
			open[parameters.Start] = startNode;

			while (openQueue.Count > 0)
			{
				PathFindingNode current = openQueue.Dequeue();
				if (closed.Contains(current.Coords))
					continue;

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

		private static void CountCharacterPoints(Character ignoredCharacter)
		{
			_characterPoints = new();

			foreach (Zone zone in World.GetZones(_box.ToRectangle()))
			{
				HashSet<Vector2> characterPoints = new();

				foreach (Character character in zone.Characters.Where(c => c != ignoredCharacter))
					characterPoints.UnionWith(character.Area.Value.GetPoints());
				_characterPoints[zone] = characterPoints;
			}
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
			foreach (Models.TileInfo tileInfo in World.Map.GetNeighbours4(parent.Coords))
			{
				Vector2 position = tileInfo.Position;
				if (position.x < _box.Min.x || position.x > _box.Max.x
					|| position.y < _box.Min.y || position.x > _box.Max.y)
					continue;

				PathFindingNode n = new(position, cost: 0, parent: null);
				yield return n;
			}
		}

		/// <summary>
		/// Zkontroluje, zda je tile průchozí atd.
		/// </summary>
		private static bool IsWalkable(PathFindingNode node, PathfindingParams parameters)
		{
			Rectangle area = Rectangle.FromCenter(
				node.Coords,
				parameters.IgnoredCharacter.Area.Value.Height,
				parameters.IgnoredCharacter.Area.Value.Width);
			Zone zone = World.Map[area.Center].Zone;

			bool walkableTerrain = true;

			foreach (Vector2 point in area.GetPoints(TileMap.TileSize))
			{
				Tile? tile = World.Map[point];

				if (tile is not { Walkable: true })
				{
					walkableTerrain = false;
					break;
				}
			}

			bool noCharacters = !(_characterPoints.ContainsKey(zone) && _characterPoints[zone].Contains(node.Coords));

			if (!walkableTerrain || !noCharacters)
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
