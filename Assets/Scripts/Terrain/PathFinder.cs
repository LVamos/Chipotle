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
		// Lokální proměnné, do kterých uložíme bounding box při hledání
		private float _minX, _maxX, _minY, _maxY;

		private List<Rectangle> _localities;

		private HashSet<Vector2> _nonpassables;

		/// <summary>
		/// Najde cestu.
		/// </summary>
		public Queue<Vector2> FindPath(
			Vector2 start,
			Vector2 goal,
			bool throughObjects = false,
			bool throughClosedDoors = true,
			bool throughImpermeableTerrain = false,
			bool sameLocality = false,
			bool throughStart = false,
			bool throughGoal = false
		)
		{
			int counter = 0, whileCounter = 0;
			if (sameLocality &&
				(World.GetLocality(start) != World.GetLocality(goal)))
				return null;

			// Vypočítám bounding box
			// Můžeš si sem dát parametr margin (rezerva), tady jen "natvrdo" 5 dlaždic
			float margin = 2f;
			_minX = Mathf.Min(start.x, goal.x) - margin;
			_maxX = Mathf.Max(start.x, goal.x) + margin;
			_minY = Mathf.Min(start.y, goal.y) - margin;
			_maxY = Mathf.Max(start.y, goal.y) + margin;

			// nonpassables
			Locality locality = World.GetLocality(start);
			string localityCode = locality.Name.Indexed.Substring(locality.Name.Indexed.Length - 2);
			Locality[] localities = World.GetLocalities()
				.Where(l => l.Name.Indexed.EndsWith(localityCode))
				.ToArray();

			_nonpassables = new();
			foreach (Locality l in localities)
				_nonpassables.UnionWith(l.Nonpassables);

			// Start uzel
			PathFindingNode startNode = new(start, cost: 0, parent: null);
			startNode.ComputeDistance(goal);

			// PriorityQueue pro nejlevnější uzel (Price)
			PriorityQueue<PathFindingNode, float> openQueue = new();
			// Dictionary na rychlé zjištění, jestli tam už node je
			Dictionary<Vector2, PathFindingNode> openDict = new();
			// Množina uzavřených uzlů
			HashSet<Vector2> closed = new();

			// Vložím start
			openQueue.Enqueue(startNode, startNode.Price);
			openDict[start] = startNode;

			while (openQueue.Count > 0)
			{
				System.Diagnostics.Debug.WriteLine($"while {++whileCounter}");
				PathFindingNode current = openQueue.Dequeue();
				openDict.Remove(current.Coords);

				// Jestli je current blízko cíle, vracím cestu
				if (Math.Abs(current.Coords.x - goal.x) <= 1 &&
					Math.Abs(current.Coords.y - goal.y) <= 1)
				{
					return GetPath(current, throughStart, throughGoal);
				}

				closed.Add(current.Coords);

				// Sousedé
				foreach (PathFindingNode neighbour in GetNeighbours(current,
					start, goal,
					throughObjects, throughClosedDoors, throughImpermeableTerrain,
					sameLocality, throughStart, throughGoal))
				{
					// Není walkable nebo je už uzavřený => skip
					if (!IsWalkable(neighbour, start, goal,
							throughObjects, throughClosedDoors,
							throughImpermeableTerrain, sameLocality,
							throughStart, throughGoal)
						|| closed.Contains(neighbour.Coords))
						continue;
					// Zkusím, jestli je to nový node, nebo tam už je
					float newCost = current.Cost + 1;
					if (!openDict.TryGetValue(neighbour.Coords, out PathFindingNode existing))
					{
						neighbour.Cost = newCost;
						neighbour.Parent = current;
						neighbour.ComputeDistance(goal);

						openQueue.Enqueue(neighbour, neighbour.Price);
						openDict[neighbour.Coords] = neighbour;
						System.Diagnostics.Debug.WriteLine((++counter).ToString());

					}
					else
					{
						// Aktualizace, pokud je teď levnější cesta
						if (newCost < existing.Cost)
						{
							existing.Cost = newCost;
							existing.Parent = current;
							existing.ComputeDistance(goal);

							// Znovu vložím do PQ, abych updatoval prioritu
							openQueue.Enqueue(existing, existing.Price);
						}
					}
				}
			}

			// Nic nenalezeno
			return null;
		}

		/// <summary>
		/// Metoda, která vybere "lokality" do listu _localities pro IsWalkable
		/// </summary>
		private void FindArea(Vector2 point)
		{
			string localityName = World.GetLocality(point).Name.Indexed;
			string areaCode = localityName.Substring(localityName.Length - 2);
			_localities = World.GetLocalities(new(point))
				.Where(l => l.Name.Indexed.Substring(l.Name.Indexed.Length - 2) == areaCode)
				.Select(l => l.Area.Value)
				.ToList();
		}

		/// <summary>
		/// Metoda, která vytváří sousedy (4-směrné)
		/// </summary>
		private IEnumerable<PathFindingNode> GetNeighbours(
			PathFindingNode parent,
			Vector2 start,
			Vector2 goal,
			bool throughObjects,
			bool throughClosedDoors,
			bool throughImpermeableTerrain,
			bool sameLocality,
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
			bool throughObjects,
			bool throughClosedDoors,
			bool throughImpermeableTerrain,
			bool sameLocality,
			bool throughStart,
			bool throughGoal
		)
		{
			// Start/goal můžou být "průchozí", i kdyby tam byla překážka
			if ((throughStart && node.Coords == start) ||
				(throughGoal && node.Coords == goal))
				return true;

			Tile tile = World.Map[node.Coords];
			return tile != null && tile.Walkable && !_nonpassables.Contains(node.Coords);
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

			if (throughStart)
				path.Dequeue();

			return path;
		}
	}
}
