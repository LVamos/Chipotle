using Luky;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// Implementation of A* algorithm
    /// </summary>
    public class PathFinder
    {

        /// <summary>
        /// Reprezents one node in a graph
        /// </summary>
        private class Node
        {
            public Vector2 Coords { get; private set; }
            public int Cost { get; private set; }
            public int Distance { get; private set; }
            public int Priority => Cost + Distance;
            public Node Parent { get; private set; }


            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="coords">Coordinates of the node</param>
            /// <param name="cost">Distance from current node</param>
            /// <param name="parent">Parent node</param>
            public Node(Vector2 coords, int cost = 0, Node parent = null)
            {
                Coords = coords;
                Cost = cost;
                Parent = parent;
            }

            /// <summary>
            /// Calculates distance between this node and the lastnode
            /// </summary>
            /// <param name="goal">The last point of requested path</param>
            public void ComputeDistance(Vector2 goal)
                => Distance = (int)(Math.Abs(goal.X - Coords.X) + Math.Abs(goal.Y - Coords.Y));
        }

        /// <summary>
        /// Constructs the shortest possible path between two points
        /// </summary>
        /// <param name="start">First point</param>
        /// <param name="end">Second point</param>
        /// <returns>The shortest path from start to end stored in List<Vector2>. If no possible path exists then it returns null.</Vector2></returns>
        public Queue<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            Queue<Vector2> GetPath(Node lastStep)
            {
                Queue<Vector2> coords = new Queue<Vector2>();

                for (Node step = lastStep.Parent; step != null; step = step.Parent)
                {
                    coords.Enqueue(step.Coords);
                }

                return new Queue<Vector2>(coords.Reverse());
            }


            Node first = new Node(start);
            Node last = new Node(end);
            first.ComputeDistance(last.Coords);

            List<Node> open = new List<Node>();
            open.Add(first);
            List<Node> closed = new List<Node>();

            while (open.Any())
            {
                Node node = open.OrderByDescending(n => n.Priority).Last();

                if (node.Coords == last.Coords)
                {
                    return GetPath(node);
                }

                closed.Add(node);
                open.Remove(node);

                IEnumerable<Node> neighbours = GetNeighbours(node, last);

                foreach (Node neighbour in neighbours)
                {
                    if (closed.Any(n => n.Coords == neighbour.Coords))
                    {
                        continue;
                    }

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
                    {
                        open.Add(neighbour);
                    }
                }
            }

            return null;
        }

        private IEnumerable<Node> GetNeighbours(Node parent, Node goal)
            => World.Map[parent.Coords].GetNeighbours4()
            .Where(t => t.Permeable && !t.IsOccupied)
            .Select(t => { Node n = new Node(t.Position, parent.Cost + 1, parent); n.ComputeDistance(goal.Coords); return n; });
    }
}
