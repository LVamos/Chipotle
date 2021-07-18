using Luky;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// A* algorithm
    /// </summary>
    class PathFinder
    {
        private class Node
        {
            public Vector2 Position { get; set; }
            public int Distance { get; set; }
            public int Cost { get; set; }
            public int Priority { get => Cost + Distance; }
            public Node Parent { get; set; }

            public void ComputeDistance(Vector2 point)
=> Distance = (int)Math.Abs(Position.X - point.X) + (int)Math.Abs(Position.Y - point.Y);

            public Node(Vector2 position, Node parent = null)
            {
                Position = position;
                Parent = parent;
            }

            public List<Node> GetNeighbours(Vector2 goal)
            {
                List<Node> nodes = new List<Node>();
                foreach (Tile t in World.Map[Position].GetNeighbours4().Where(t => t.Permeable && !t.IsOccupied))
                {
                    Node n = new Node(t.Position, this) { Cost = this.Cost + 1 };
                    n.ComputeDistance(goal);
                    nodes.Add(n);
                }
                return nodes;
            }
        }




        public (bool found, Queue<Vector2> path) FindPath(Vector2 start, Vector2 goal)
        {

            List<Node> active = new List<Node>();
            List<Node> visited = new List<Node>();
            Node first = new Node(start);
            Node last = new Node(goal);
            first.ComputeDistance(goal);

            active.Add(first);

            Node node = null;
            while (active.Count > 0)
            {
                int highestPriority = active.Min(n => n.Priority);
                node = active.First(n => n.Priority == highestPriority);

                visited.Add(node);

                if (visited.FirstOrDefault(n => n.Position == goal) != null)
                    break;

                active.Remove(node);

                foreach (Node neighbour in node.GetNeighbours(goal))
                {
                    if (visited.FirstOrDefault(n => n.Position == neighbour.Position) != null)
                        continue;

                    if (active.FirstOrDefault(n => n.Position == neighbour.Position) == null)
                        active.Insert(0, neighbour);
                    else
                    {
                        Node compared = active.First(n => n.Position == neighbour.Position);

                        if (compared.Priority > node.Priority)
                        {
                            active.Remove(compared);
                            active.Add(neighbour);
                        }
                        else active.Add(neighbour);
                    }
                }
            }

            // Reconstruct path
            Queue<Vector2> coords = new Queue<Vector2>();
            while (node != null)
            {
                coords.Enqueue(node.Position);
                node = node.Parent;
            }

            return (false, coords);
        }

    }
}
