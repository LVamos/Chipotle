using System;

using UnityEngine;

namespace Game.Terrain
{
	/// <summary>
	/// Reprezents one node in a graph corresponding to a tile on a tile map.
	/// </summary>
	public class PathFindingNode
	{
		/// <summary>
		/// Checks if there's a closed door on this node.
		/// </summary>
		/// <returns>True if there's a closed door on the node</returns>
		public bool IsClosedDoor()
		{
			Passage p = World.GetPassage(Coords);
			return p is { State: PassageState.Closed };
		}

		/// <summary>
		/// Checks if there's an object or entity on this node.
		/// </summary>
		/// <returns>True if there's an object or entity on the node</returns>
		public bool IsObjectOrCharacter()
		{
			return World.IsOccupied(Coords);
		}

		/// <summary>
		/// Checks if there's an impermeable terrain on this node.
		/// </summary>
		/// <returns>True if there's an object or entity on the node</returns>
		public bool IsImpermeableTerrain()
		{
			return !World.Map[Coords].Walkable;
		}

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="coords">Coordinates of the node</param>
		/// <param name="cost">Distance from current node</param>
		/// <param name="parent">The parent node</param>
		public PathFindingNode(Vector2 coords, int cost = 0, PathFindingNode parent = null)
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
		public float Cost { get; set; }

		/// <summary>
		/// Heuristic function expressing distance between this node and the goal node.
		/// </summary>
		public int Distance { get; private set; }

		/// <summary>
		/// The parrent of this node
		/// </summary>
		public PathFindingNode Parent { get; set; }

		/// <summary>
		/// A heuristic function expressing length of potential path from the start to the goal across this node.
		/// </summary>
		public float Price => Cost + Distance;

		/// <summary>
		/// Calculates distance between this node and the lastnode
		/// </summary>
		/// <param name="goal">The last point of requested path</param>
		public void ComputeDistance(Vector2 goal)
		{
			Distance = (int)(Math.Abs(goal.x - Coords.x) + Math.Abs(goal.y - Coords.y));
		}
	}
}
