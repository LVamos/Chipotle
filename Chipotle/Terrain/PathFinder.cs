using System;
using System.Collections.Generic;
using System.Linq;

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
        public Queue<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            Queue<Vector2> GetPath(Node lastStep)
            {
                Queue<Vector2> coords = new Queue<Vector2>();

                for (Node step = lastStep.Parent; step != null; step = step.Parent)
                    coords.Enqueue(step.Coords);

                return new Queue<Vector2>(coords.Reverse());
            }

            Node first = new Node(start);
            Node last = new Node(end);
            first.ComputeDistance(last.Coords);

            List<Node> open = new List<Node>
            {
                first
            };
            List<Node> closed = new List<Node>();

            while (open.Any())
            {
                Node node = open.OrderByDescending(n => n.Priority).Last();

                if (node.Coords == last.Coords)
                    return GetPath(node);

                closed.Add(node);
                open.Remove(node);

                IEnumerable<Node> neighbours = GetNeighbours(node, last);

                foreach (Node neighbour in neighbours)
                {
                    if (closed.Any(n => n.Coords == neighbour.Coords))
                        continue;

                    if (open.Any(n => n.Coords == neighbour.Coords))
                    {
                        Node test = open.First(n => n.Coords == neighbour.Coords);

                        if (test.Priority > node.Priority)
                        {
                            open.Remove(test);
                            open.Add(neighbour);
                        }
                    }
                    else
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
            /// Distance from current node
            /// </summary>
            public int Cost { get; private set; }

            /// <summary>
            /// Distance between this node and the goal node
            /// </summary>
            public int Distance { get; private set; }

            /// <summary>
            /// The parrent of this node
            /// </summary>
            public Node Parent { get; private set; }

            /// <summary>
            /// A heuristic parameter
            /// </summary>
            public int Priority => Cost + Distance;

            /// <summary>
            /// Calculates distance between this node and the lastnode
            /// </summary>
            /// <param name="goal">The last point of requested path</param>
            public void ComputeDistance(Vector2 goal)
                => Distance = (int)(Math.Abs(goal.X - Coords.X) + Math.Abs(goal.Y - Coords.Y));
        }
    }
}