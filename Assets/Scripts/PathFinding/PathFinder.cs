using Game.Entities.Characters;
using Game.Terrain;

using System.Collections.Generic;
using System.Linq;

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
					return SplitIntoSteps(GetPath(current, parameters.ThroughStart, parameters.ThroughGoal), parameters);

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
			if (!IsFootprintWalkable(node.Coords, parameters))
				return false;

			// If the node lays on start or goal, exclude it if not allowed.
			if ((!parameters.ThroughStart && node.Coords == parameters.Start) ||
				(!parameters.ThroughGoal && node.Coords == parameters.Goal))
				return false;

			return true;
		}

		/// <summary>
		/// Checks if the character's footprint centered at the given position covers only walkable positions.
		/// </summary>
		private static bool IsFootprintWalkable(Vector2 center, PathfindingParams parameters)
		{
			Rectangle area = Rectangle.FromCenter(center, parameters.CharacterHeight, parameters.CharacterWidth);

			foreach (Vector2 point in area.GetPoints(TileMap.TileSize))
			{
				if (!parameters.Snapshot.WalkablePositions.Contains(point))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if the character can walk straight from one position to another.
		/// </summary>
		private static bool IsLineWalkable(Vector2 from, Vector2 to, PathfindingParams parameters)
		{
			float distance = Vector2.Distance(from, to);
			int samples = Mathf.CeilToInt(distance / _lineSamplingStep);

			for (int i = 1; i <= samples; i++)
			{
				if (!IsFootprintWalkable(Vector2.Lerp(from, to, (float)i / samples), parameters))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Distance between the positions checked along a straight line.
		/// </summary>
		private const float _lineSamplingStep = .1f;

		/// <summary>
		/// Straightens the grid path and splits it into steps of the character's step length.
		/// </summary>
		/// <remarks>
		/// The grid nodes are only <see cref="TileMap.TileSize"/> apart and the path zigzags in 4 directions.
		/// Every step is checked against the same walkable positions as the grid search, so it never crosses an obstacle.
		/// </remarks>
		private static Queue<Vector2> SplitIntoSteps(Queue<Vector2> path, PathfindingParams parameters)
		{
			float stepLength = parameters.StepLength;
			if (path.IsNullOrEmpty() || stepLength <= 0)
				return path;

			List<Vector2> nodes = new() { parameters.Start };
			nodes.AddRange(path.Where(p => p != parameters.Start));
			List<Vector2> corners = GetCorners(nodes, parameters);

			Queue<Vector2> steps = new();
			if (parameters.ThroughStart)
				steps.Enqueue(parameters.Start);

			Vector2 position = corners[0];
			int index = 1;
			while (index < corners.Count)
			{
				Vector2 corner = corners[index];
				float distance = Vector2.Distance(position, corner);

				if (distance > stepLength)
				{
					position += (corner - position) / distance * stepLength;
					steps.Enqueue(position);
					continue;
				}

				bool lastCorner = index == corners.Count - 1;
				if (lastCorner)
				{
					if (distance > _minStepLength)
						steps.Enqueue(corner);
					break;
				}

				// The corner is closer than one step, so try to finish the step behind it.
				Vector2 next = corners[index + 1];
				float rest = Mathf.Min(stepLength - distance, Vector2.Distance(corner, next));
				Vector2 candidate = corner + (next - corner).normalized * rest;
				position = IsLineWalkable(position, candidate, parameters) ? candidate : corner;
				steps.Enqueue(position);
				index++;
			}

			return steps.Count > 0 ? steps : path;
		}

		/// <summary>
		/// Shortest step that is worth making at the end of a path.
		/// </summary>
		private const float _minStepLength = .05f;

		/// <summary>
		/// Leaves only the nodes where the path has to turn, so that the character walks straight between them.
		/// </summary>
		private static List<Vector2> GetCorners(List<Vector2> nodes, PathfindingParams parameters)
		{
			List<Vector2> corners = new() { nodes[0] };
			int anchor = 0;

			for (int i = 2; i < nodes.Count; i++)
			{
				if (!IsLineWalkable(nodes[anchor], nodes[i], parameters))
				{
					anchor = i - 1;
					corners.Add(nodes[anchor]);
				}
			}

			if (nodes.Count > 1)
				corners.Add(nodes[^1]);
			return corners;
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
