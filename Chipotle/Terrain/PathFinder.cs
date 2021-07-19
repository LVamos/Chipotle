using System.Text;
using Game.Terrain;
using Luky;
using System;
using System.Collections.Generic;
using System.Linq;

//A* Search Pathfinding Example from : https://dotnetcoretutorials.com/2020/07/25/a-search-pathfinding-algorithm-in-c/ 
namespace Game.Terrain
{
    public class PathFinder
    {
        public List<Vector2> FindPath(Vector2 initial, Vector2 final)
        {
            StringBuilder b = new StringBuilder();

            var start = new Node();
            start.Y = (int)initial.Y;
            start.X = (int)initial.X;


            var finish = new Node();
            finish.Y = (int)final.Y;
            finish.X = (int)final.X;

            start.SetDistance(finish.X, finish.Y);

            var activeNodes = new List<Node>();
            activeNodes.Add(start);
            var visitedNodes = new List<Node>();

            while (activeNodes.Any())
            {
                var checkNode = activeNodes.OrderBy(x => x.CostDistance).First();

                if (checkNode.X == finish.X && checkNode.Y == finish.Y)
                {
                    //We found the destination and we can be sure (Because the the OrderBy above)
                    //That it's the most low cost option. 
                    var node = checkNode;
                    b.AppendLine("Retracing steps backwards...");
                    List<Vector2> coords = new List<Vector2>();
                    while (true)
                    {
                        b.AppendLine($"{node.X} : {node.Y}");
                        coords.Add(new Vector2(node.X, node.Y));
                        node = node.Parent;
                        if (node == null)
                        {
                            b.AppendLine("Done!");
                            System.Windows.Forms.Clipboard.SetText(b.ToString());
                            return coords;
                        }
                    }
                }

                visitedNodes.Add(checkNode);
                activeNodes.Remove(checkNode);

                var walkableNodes = GetWalkableNodes(checkNode, finish);

                foreach (var walkableNode in walkableNodes)
                {
                    //We have already visited this node so we don't need to do so again!
                    if (visitedNodes.Any(x => x.X == walkableNode.X && x.Y == walkableNode.Y))
                        continue;

                    //It's already in the active list, but that's OK, maybe this new node has a better value (e.g. We might zigzag earlier but this is now straighter). 
                    if (activeNodes.Any(x => x.X == walkableNode.X && x.Y == walkableNode.Y))
                    {
                        var existingNode = activeNodes.First(x => x.X == walkableNode.X && x.Y == walkableNode.Y);
                        if (existingNode.CostDistance > checkNode.CostDistance)
                        {
                            activeNodes.Remove(existingNode);
                            activeNodes.Add(walkableNode);
                        }
                    }
                    else
                    {
                        //We've never seen this node before so add it to the list. 
                        activeNodes.Add(walkableNode);
                    }
                }
            }

            b.AppendLine("No Path Found!");
            System.Windows.Forms.Clipboard.SetText(base.ToString());
            return null;
        }

        private List<Node> GetWalkableNodes(Node currentNode, Node targetNode)
        {
            var possibleNodes = new List<Node>()
            {
                new Node { X = currentNode.X, Y = currentNode.Y - 1, Parent = currentNode, Cost = currentNode.Cost + 1 },
                new Node { X = currentNode.X, Y = currentNode.Y + 1, Parent = currentNode, Cost = currentNode.Cost + 1},
                new Node { X = currentNode.X - 1, Y = currentNode.Y, Parent = currentNode, Cost = currentNode.Cost + 1 },
                new Node { X = currentNode.X + 1, Y = currentNode.Y, Parent = currentNode, Cost = currentNode.Cost + 1 },
            };

            possibleNodes.ForEach(node => node.SetDistance(targetNode.X, targetNode.Y));


            return possibleNodes
                    .Where(node => World.Map[node.X, node.Y]!=null && World.Map[node.X, node.Y].Permeable && !World.Map[node.X, node.Y].IsOccupied)
                    .ToList<Node>();
        }
    }

    class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Cost { get; set; }
        public int Distance { get; set; }
        public int CostDistance => Cost + Distance;
        public Node Parent { get; set; }

        //The distance is essentially the estimated distance, ignoring walls to our target. 
        //So how many nodes left and right, up and down, ignoring walls, to get there. 
        public void SetDistance(int targetX, int targetY)
        {
            this.Distance = Math.Abs(targetX - X) + Math.Abs(targetY - Y);
        }
    }
}
