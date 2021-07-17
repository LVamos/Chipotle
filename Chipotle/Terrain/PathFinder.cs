using Luky;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Terrain
{
    /// <summary>
    /// A* algorithm
    /// </summary>
    class PathFinder
    {
        private class Node
        {
            public Vector2 Position;
            public int Distance;
            public int Cost { get; set; }
            public int Priority { get; set; }
            public Node Parent;

            public void ComputeDistance(Vector2 point)
=> Distance = (int)Math.Abs(Position.X - point.X) + (int)Math.Abs(Position.Y - point.Y);

            public Node(Vector2 position, Node parent = null)
            {
                Position = position;
                parent = parent;
            }

            public IEnumerable<Node> GetNeighbours(Vector2 goal)
            {
                foreach (Tile t in World.Map[Position].GetNeighbours4().Where(t => t.Permeable && !t.IsOccupied))
                {
                    Node n = new Node(t.Position, this) { Cost = this.Cost + 1 };
                    n.ComputeDistance(goal);
                    yield return n;
                }
            }
        }




        public (bool found, List<Vector2> path) FindPath(Vector2 start, Vector2 goal)
        {

        List<Node> active = new List<Node>();
        List<Node> visited = new List<Node>();
            Node last = new Node(goal);
            Node first = new Node(start);
            first.ComputeDistance(goal);

            active.Add(first);

            Node node=null;
            while(!active.IsNullOrEmpty())
            {
                node = active.OrderBy(n => n.Priority).First();

                if (node.Position == goal)
                    break;

                visited.Add(node);
                active.Remove(node);
                    foreach(Node neighbour in  node.GetNeighbours(goal))
                    {
                        if (visited.Any(n => n.Position == neighbour.Position))
                            continue;

                        Node compared = active.FirstOrDefault(n => n.Position == neighbour.Position);
                        if (compared!=null)
                        {
                            if (node.Priority < neighbour.Priority)
                            {
                                active.Remove(compared);
                                active.Add(neighbour);
                            }
                            else active.Add(neighbour);
                        }
                    }
            }

            // Reconstruct path
                List<Vector2> coords = new List<Vector2>();
                while (node!= null)
                {
                    coords.Add(node.Position);
                    node = node.Parent;
                }

            return (false, coords);
    }

}
}
