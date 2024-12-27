using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Terrain
{
	/// <summary>
	/// Implementation of A* algorithm
	/// </summary>
	[Serializable]
	public class PathFinder
	{
		/// <summary>
		/// Reconstructs a path from the last pathFindingNode.
		/// </summary>
		/// <param name="lastPoint">The last point of the path</param>
		/// <param name="throughStart">Specifies if the first point shoudl be included</param>
		/// <param name="throughGoal">Specifies if the last poitn should be included.</param>
		/// <returns></returns>
		private Queue<Vector2> GetPath(PathFindingNode lastPoint, bool throughStart, bool throughGoal)
		{
			Queue<Vector2> coords = new();

			for (PathFindingNode step = lastPoint; step != null; step = step.Parent)
				coords.Enqueue(step.Coords);

			Queue<Vector2> path = new(coords.Reverse());

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

		/// <summary>
		/// Tests if the specified pathFindingNode is walkable and in distance limit.
		/// </summary>
		/// <param name="pathFindingNode">The pathFindingNode to be checked</param>
		/// <param name="start">The initial point fo path finding.</param>
		/// <param name="goal">The goal of the path finding</param>
		/// <param name="throughObjects">Specifies if tiles with objects should be included.</param>
		/// <param name="throughClosedDoors"Specifies if tiles on closed doors should be included.></param>
		/// <param name="throughImpermeableTerrain">Specifies if tiles with impermeable terrain should be included.</param>
		/// <param name="sameLocality">Specifies if different localities than locality of the initial point should be included</param>
		/// <param name="throughStart">Specifies if the start point should be considered walkable.</param>
		/// <param name="throughGoal">Specifies if the goal should be considered walkable</param>
		/// <param name="maxDistance">Maximum allowed distance from the initial point</param>
		/// <returns>True if the tile is walkable</returns>
		private bool IsWalkable(PathFindingNode pathFindingNode, Vector2 start, Vector2 goal, bool throughObjects, bool throughClosedDoors, bool throughImpermeableTerrain, bool sameLocality, bool throughStart, bool throughGoal, int maxDistance)
		{
			if (
				(throughStart && pathFindingNode.Coords == start)
				|| (throughGoal && pathFindingNode.Coords == goal)
			)
				return true;

			return
				World.Map[pathFindingNode.Coords] != null
				&& (!sameLocality || (sameLocality && World.GetLocality(start) == World.GetLocality(pathFindingNode.Coords)))
				&& pathFindingNode.Distance <= maxDistance
				&& (throughObjects || (!throughObjects && !pathFindingNode.IsObjectOrEntity()))
				&& (throughClosedDoors || (!throughClosedDoors && !pathFindingNode.IsClosedDoor()))
				&& (throughImpermeableTerrain || (!throughImpermeableTerrain && !pathFindingNode.IsImpermeableTerrain()));
		}


		/// <summary>
		/// Constructs the shortest possible path between two points.
		/// </summary>
		/// <param name="start">The initial point fo path finding.</param>
		/// <param name="goal">The goal of the path finding</param>
		/// <param name="throughObjects">Specifies if tiles with objects should be included.</param>
		/// <param name="throughClosedDoors"Specifies if tiles on closed doors should be included.></param>
		/// <param name="throughImpermeableTerrain">Specifies if tiles with impermeable terrain should be included.</param>
		/// <param name="sameLocality">Specifies if different localities than locality of the initial point should be included</param>
		/// <param name="throughStart">Specifies if the start point should be considered walkable.</param>
		/// <param name="throughGoal">Specifies if the goal should be considered walkable</param>
		/// <param name="maxDistance">Maximum allowed distance from the initial point</param>
		/// <returns>
		/// A list of points leading from start to the end or null if no possible path exists
		/// </returns>
		public Queue<Vector2> FindPath(Vector2 start, Vector2 goal, bool throughObjects = false, bool throughClosedDoors = true, bool throughImpermeableTerrain = false, bool sameLocality = false, bool throughStart = false, bool throughGoal = false, int maxDistance = 300)
		{
			// Detect irrelevant requests
			if (sameLocality &&
				(World.GetLocality(start) != World.GetLocality(goal)))
				return null;

			// Find the path.
			// Začni výchozím uzlem.
			PathFindingNode first = new(start);
			PathFindingNode last = new(goal);
			first.ComputeDistance(last.Coords);

			// Přidej výchozí uzel do seznamu otevřených uzlů.
			List<PathFindingNode> open = new() { first };
			List<PathFindingNode> closed = new();

			// Procházej seznam otevřených uzlů, dokud se nevyprázdní.
			while (open.Any())
			{
				// Z otevřených uzlů vyber ten nejlevnější.
				PathFindingNode pathFindingNode = open.OrderByDescending(n => n.Price).Last();

				// Pokud už jsme v cíli, skonči.
				if (pathFindingNode.Coords == last.Coords)
					return GetPath(pathFindingNode, throughStart, throughGoal);

				// Jinak přesuň uzel z otevřených do uzavřených.
				closed.Add(pathFindingNode);
				open.Remove(pathFindingNode);

				// Projdi sousedy
				foreach (PathFindingNode neighbour in GetNeighbours(pathFindingNode, start, goal, throughObjects, throughClosedDoors, throughImpermeableTerrain, sameLocality, throughStart, throughGoal, maxDistance))
				{
					// Pokud soused patří mezi uzavřené nebo je na něm nepovolená překážka, přeskoč ho.
					if (
						(!IsWalkable(neighbour, start, goal, throughObjects, throughClosedDoors, throughImpermeableTerrain, sameLocality, throughStart, throughGoal, maxDistance))
						|| closed.Any(c => c.Coords == neighbour.Coords)
					)
						continue;

					// Jinak pokud je v otevřených uzel, který má stejné souřadnice jako soused, ale je dražší, nahraď ho sousedem. v opačném případě jen přidej souseda mezi otevřené.
					PathFindingNode openPathFindingNode = open.FirstOrDefault(o => o.Coords == neighbour.Coords);
					if (openPathFindingNode != null && openPathFindingNode.Price > pathFindingNode.Price)
						open.Remove(openPathFindingNode);

					open.Add(neighbour);
				}
			}

			return null;
		}

		/// <summary>
		/// Returns all adjacent tiles around the <paramref name="parent"/>.
		/// </summary>
		/// <param name="parent">The default tile</param>
		/// <param name="goal">The target tile</param>
		/// <returns>Enumeration of all adjacent neighbours</returns>
		private IEnumerable<PathFindingNode> GetNeighbours(PathFindingNode parent, Vector2 start, Vector2 goal, bool throughObjects, bool throughClosedDoors, bool throughImpermeableTerrain, bool sameLocality, bool throughStart, bool throughGoal, int maxDistance)
		{
			IEnumerable<PathFindingNode> neighbours =
				from neighbour in World.Map.GetNeighbours4(parent.Coords)
				select new PathFindingNode(neighbour.position, parent.Cost + 1, parent);

			foreach (PathFindingNode n in neighbours)
			{
				n.ComputeDistance(goal);
				if (IsWalkable(n, start, goal, throughObjects, throughClosedDoors, throughImpermeableTerrain, sameLocality, throughStart, throughGoal, maxDistance))
					yield return n;
			}
		}
	}
}