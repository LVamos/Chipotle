using Game.Entities.Characters;
using Game.Terrain;

using System.Collections.Generic;

using UnityEngine;

namespace Game.PathFinding
{
	/// <summary>
	/// Implementation of A* algorithm with PriorityQueue + Dictionary for open set,
	/// plus a bounding box to limit the search area.
	/// </summary>
	/// <remarks>
	/// <see cref="FindPath"/> is a pure function of its <see cref="PathfindingParams"/> argument - no
	/// static mutable state, and no reads of <c>World</c>/<c>Zone</c>/<c>Character</c> - so it's safe to
	/// call from a background thread (e.g. via <c>Task.Run</c>). All the live-world reading happens up
	/// front, synchronously, on the calling (main) thread, in <see cref="BuildSnapshot"/>.
	/// </remarks>
	public static class PathFinder
	{
		private const float _distanceToleration = .5f;
		private const float _boundingBoxMargin = 5;

		// Left, Up, Right, Down - same 4 directions as DirectionExtension.BasicDirections, but expressed
		// as plain math instead of going through TileMap.GetNeighbours4 (which reads World.Map and
		// therefore isn't safe to call once a search is running on a background thread).
		private static readonly Vector2[] _cardinalOffsets = { new(-1, 0), new(0, 1), new(1, 0), new(0, -1) };

		/// <summary>
		/// Builds a thread-safe snapshot of the world data needed to find a path between
		/// <paramref name="start"/> and <paramref name="goal"/>.
		/// </summary>
		/// <remarks>
		/// Must be called on the main thread - reads <c>World</c>, <c>Zone</c> and
		/// <paramref name="ignoredCharacter"/> live. Once this returns, the resulting
		/// <see cref="PathfindingSnapshot"/> is safe to hand to a background thread: nothing in it is
		/// shared with anything the main thread can still mutate.
		/// </remarks>
		/// <param name="ignoredCharacter">
		/// The character the path is being found for. Only its current footprint size/position is read
		/// here, on the main thread; the reference itself is never touched again after this call, so the
		/// search doesn't care if this character keeps moving on the main thread while it runs.
		/// </param>
		public static PathfindingSnapshot BuildSnapshot(Vector2 start, Vector2 goal, Character ignoredCharacter, float margin = _boundingBoxMargin)
		{
			BoundingBox box = BoundingBox.Create(start, goal, margin);
			Rectangle boxRect = box.ToRectangle();

			// Positions currently occupied by some other character's footprint - checked once here,
			// instead of on every node visited during the search (which is what made this expensive
			// before it was snapshotted).
			HashSet<Vector2> blockedByCharacters = new();
			foreach (Zone zone in World.GetZones(boxRect))
			{
				foreach (Character character in zone.Characters)
				{
					if (character == ignoredCharacter || character.Area == null)
						continue;

					// Don't let a character block the very point we're trying to reach - if the goal
					// lies inside their own footprint (e.g. the goal IS that character's position,
					// as with GoToPlayer), that's who we're trying to get to, not an obstacle.
					if (character.Area.Value.Contains(goal))
						continue;

					blockedByCharacters.UnionWith(character.Area.Value.GetPoints());
				}
			}

			// Single-tile walkability: terrain passable, not covered by a static obstacle
			// (Zone.IsWalkable / Zone._nonwalkables), and not occupied by another character.
			HashSet<Vector2> walkable = new();
			foreach (Vector2 point in boxRect.GetPoints(TileMap.TileSize))
			{
				Tile tile = World.Map[point];
				if (tile is not { Walkable: true })
					continue;

				if (tile.Zone != null && !tile.Zone.IsWalkable(point))
					continue;

				if (blockedByCharacters.Contains(point))
					continue;

				walkable.Add(point);
			}

			return new PathfindingSnapshot(box, walkable);
		}

		/// <summary>
		/// Najde cestu. Pure function of <paramref name="parameters"/> - safe to call from any thread.
		/// </summary>
		public static Queue<Vector2> FindPath(PathfindingParams parameters)
		{
			BoundingBox box = parameters.Snapshot.Box;

			PathFindingNode startNode = new(parameters.Start, cost: 0, parent: null);
			startNode.GetDistance(parameters.Goal);

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
				if (IsAtGoal(current.Coords, parameters.Goal))
					return GetPath(current, parameters.ThroughStart, parameters.ThroughGoal);

				closed.Add(current.Coords);

				foreach (PathFindingNode neighbour in GetNeighbours(current, box))
				{
					// Is not walkable or is already closed => skip
					if (closed.Contains(neighbour.Coords) || !IsWalkable(neighbour, parameters))
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
						neighbour.GetDistance(parameters.Goal);

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
							existing.GetDistance(parameters.Goal);

							// re -put in PQ to update the priority
							openQueue.Enqueue(existing, existing.Price);
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Checks if the position is close enough to the goal to count as reached.
		/// </summary>
		public static bool IsAtGoal(Vector2 position, Vector2 goal)
			=> Vector2.Distance(position, goal) <= _distanceToleration;

		/// <summary>
		/// Metoda, která vytváří sousedy (4-směrné)
		/// </summary>
		private static IEnumerable<PathFindingNode> GetNeighbours(PathFindingNode parent, BoundingBox box)
		{
			foreach (Vector2 offset in _cardinalOffsets)
			{
				Vector2 position = new(
					(parent.Coords.x + offset.x * TileMap.TileSize).Round(),
					(parent.Coords.y + offset.y * TileMap.TileSize).Round());

				if (position.x < box.Min.x || position.x > box.Max.x
					|| position.y < box.Min.y || position.y > box.Max.y)
					continue;

				yield return new(position, cost: 0, parent: null);
			}
		}

		/// <summary>
		/// Zkontroluje, zda je tile průchozí atd.
		/// </summary>
		private static bool IsWalkable(PathFindingNode node, PathfindingParams parameters)
		{
			Rectangle area = Rectangle.FromCenter(node.Coords, parameters.CharacterHeight, parameters.CharacterWidth);

			foreach (Vector2 point in area.GetPoints(TileMap.TileSize))
			{
				if (!parameters.Snapshot.WalkablePositions.Contains(point))
					return false;
			}

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
