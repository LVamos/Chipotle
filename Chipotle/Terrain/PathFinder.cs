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

            public void ComputeDistance(Vector2 goal)
=> Distance = (int)(Math.Abs(goal.X - Position.X) + Math.Abs(goal.Y - Position.Y));

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
            while (active.Any())
            {
                node = active.OrderByDescending(n=>n.Priority).Last();

                if (node.Position == goal)
                    break;

                visited.Add(node);
                active.Remove(node);


                List<Node> neighbours = node.GetNeighbours(goal);
                foreach (Node neighbour in neighbours)
                {
                            if (visited.Any(n => n.Position == neighbour.Position))
                        continue;

                    if (active.Any(n => n.Position == neighbour.Position))
                    {
                        Node compared = active.First(n => n.Position == neighbour.Position);
                        if (compared.Priority > node.Priority)
                        {
                            active.Remove(compared);
                            active.Add(neighbour);
                        }
                    }
                    else active.Add(neighbour);
                }
            }


            // Reconstruct path
            bool found = node.Position == goal;
            Queue<Vector2> coords = new Queue<Vector2>();
            while (node != null)
            {
                coords.Enqueue(node.Position);
                node = node.Parent;
            }
            return (found, new Queue<Vector2>(coords.Reverse()));
        }

    }
}
