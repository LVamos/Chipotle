using System;
using System.Collections.Generic;
using System.Linq;

using Game.Entities;

using OpenTK;

namespace Game.Terrain
{
	/// <summary>
	/// Implementation of A* algorithm
	/// </summary>
	[Serializable]
	public class PathFinder
	{
		/// <summary>
		/// Constructs the shortest possible path between two points.
		/// </summary>
		/// <param name="start">First point</param>
		/// <param name="end">Second point</param>
		/// <returns>
		/// A list of points leading from start to the end or null if no possible path exists
		/// </returns>
		public Queue<Vector2> FindPath(Vector2 start, Vector2 end, bool throughObjects = false, bool throughClosedDoors = true, bool throughImpermeableTerrain = false)
		{
			// Tests if a node is walkable.
			bool Inaccessible(Node n)
						=> (!throughObjects && n.IsObjectOrEntity())
						|| (!throughClosedDoors && n.IsClosedDoor())
						|| (!throughImpermeableTerrain && n.IsImpermeableTerrain());

			// Reconstructs the path.
			Queue<Vector2> GetPath(Node lastStep)
			{
				Queue<Vector2> coords = new Queue<Vector2>();

				for (Node step = lastStep.Parent; step != null; step = step.Parent)
					coords.Enqueue(step.Coords);

				return new Queue<Vector2>(coords.Reverse());
			}

			// Find the path.
			// Začni výchozím uzlem.
			Node first = new Node(start);
			Node last = new Node(end);
			first.ComputeDistance(last.Coords);

			// Přidej výchozí uzel do seznamu otevřených uzlů.
			List<Node> open = new List<Node> { first };
			List<Node> closed = new List<Node>();

			// Procházej seznam otevřených uzlů, dokud se nevyprázdní.
			while (open.Any())
			{
				if (open.Count() >= 400)
					System.Diagnostics.Debugger.Break();
				// Z otevřených uzlů vyber ten nejlevnější.
				Node node = open.OrderByDescending(n => n.Price).Last();

				// Pokud už jsme v cíli, skonči.
				if (node.Coords == last.Coords)
					return GetPath(node);

				// Jinak přesuň uzel z otevřených do uzavřených.
				closed.Add(node);
				open.Remove(node);

				// Projdi sousedy
				foreach (Node neighbour in GetNeighbours(node, last))
				{
					// Pokud soused patří mezi uzavřené nebo je na něm nepovolená překážka, přeskoč ho.
					if (
						Inaccessible(neighbour)
						|| closed.Any(c => c.Coords == neighbour.Coords)
						)
						continue;

					// Jinak pokud je v otevřených uzel, který má stejné souřadnice jako soused, ale je dražší, nahraď ho sousedem. v opačném případě jen přidej souseda mezi otevřené.
					Node openNode = open.FirstOrDefault(o => o.Coords == neighbour.Coords);
					if (openNode != null && openNode.Price > node.Price)
						open.Remove(openNode);

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
		private IEnumerable<Node> GetNeighbours(Node parent, Node goal)
					=> World.Map.GetNeighbours4(parent.Coords)
					.Where(t => t.tile.Walkable && !World.IsOccupied(t.position))
					.Select(t => { Node n = new Node(t.position, parent.Cost + 1, parent); n.ComputeDistance(goal.Coords); return n; });

		/// <summary>
		/// Reprezents one node in a graph corresponding to a tile on a tile map.
		/// </summary>
		private class Node
		{
			/// <summary>
			/// Checks if there's a closed door on this node.
			/// </summary>
			/// <returns>True if there's a closed door on the node</returns>
			public bool IsClosedDoor()
			{
				Passage p = World.GetPassage(Coords);
				return p != null && p.State == PassageState.Closed;
			}

			/// <summary>
			/// Checks if there's an object or entity on this node.
			/// </summary>
			/// <returns>True if there's an object or entity on the node</returns>
			public bool IsObjectOrEntity()
				=> World.IsOccupied(Coords);

			/// <summary>
			/// Checks if there's an impermeable terrain on this node.
			/// </summary>
			/// <returns>True if there's an object or entity on the node</returns>
			public bool IsImpermeableTerrain()
				=> !World.Map[Coords].Walkable;

			/// <summary>
			/// constructor
			/// </summary>
			/// <param name="coords">Coordinates of the node</param>
			/// <param name="cost">Distance from current node</param>
			/// <param name="parent">The parent node</param>
			public Node(Vector2 coords, int cost = 0, Node parent = null)
			{
				Coords = coords;
				Cost = cost;
				Parent = parent;
			}

			/// <summary>
			/// Coordinates of the node
			/// </summary>
			public Vector2 Coords { get; private set; }

			/// <summary>
			/// Heuristic function determining number of steps taken from the first node to this node.
			/// </summary>
			public int Cost { get; private set; }

			/// <summary>
			/// Heuristic function expressing distance between this node and the goal node.
			/// </summary>
			public int Distance { get; private set; }

			/// <summary>
			/// The parrent of this node
			/// </summary>
			public Node Parent { get; private set; }

			/// <summary>
			/// A heuristic function expressing length of potential path from the start to the goal across this node.
			/// </summary>
			public int Price => Cost + Distance;

			/// <summary>
			/// Calculates distance between this node and the lastnode
			/// </summary>
			/// <param name="goal">The last point of requested path</param>
			public void ComputeDistance(Vector2 goal)
				=> Distance = (int)(Math.Abs(goal.X - Coords.X) + Math.Abs(goal.Y - Coords.Y));
		}
	}
}