using OpenTK;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// Implementation of A* algorithm
    /// </summary>
    [Serializable]
    public partial class PathFinder
    {
        /// <summary>
        /// Reconstructs a path from the last node.
        /// </summary>
        /// <param name="lastPoint">The last point of the path</param>
        /// <param name="throughStart">Specifies if the first point shoudl be included</param>
        /// <param name="throughGoal">Specifies if the last poitn should be included.</param>
        /// <returns></returns>
        public Queue<Vector2> GetPath(Node lastPoint, bool throughStart, bool throughGoal)
        {
            Queue<Vector2> coords = new Queue<Vector2>();

            for (Node step = lastPoint; step != null; step = step.Parent)
                coords.Enqueue(step.Coords);

            Queue<Vector2> path = new Queue<Vector2>(coords.Reverse());

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
        /// Tests if the specified node is walkable and in distance limit.
        /// </summary>
        /// <param name="node">The node to be checked</param>
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
        public bool Walkable(Node node, Vector2 start, Vector2 goal, bool throughObjects, bool throughClosedDoors, bool throughImpermeableTerrain, bool sameLocality, bool throughStart, bool throughGoal, int maxDistance)
        {
            if (
                    (throughStart && node.Coords == start)
                    || (throughGoal && node.Coords == goal)
                    )
                return true;

            return
                World.Map[node.Coords] != null
                        && (!sameLocality || (sameLocality && World.GetLocality(start) == World.GetLocality(node.Coords)))
                        && node.Distance <= maxDistance
                        && (throughObjects || (!throughObjects && !node.IsObjectOrEntity()))
                    && (throughClosedDoors || (!throughClosedDoors && !node.IsClosedDoor()))
                        && (throughImpermeableTerrain || (!throughImpermeableTerrain && !node.IsImpermeableTerrain()));
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
            Node first = new Node(start);
            Node last = new Node(goal);
            first.ComputeDistance(last.Coords);

            // Přidej výchozí uzel do seznamu otevřených uzlů.
            List<Node> open = new List<Node> { first };
            List<Node> closed = new List<Node>();

            // Procházej seznam otevřených uzlů, dokud se nevyprázdní.
            while (open.Any())
            {
                // Z otevřených uzlů vyber ten nejlevnější.
                Node node = open.OrderByDescending(n => n.Price).Last();

                // Pokud už jsme v cíli, skonči.
                if (node.Coords == last.Coords)
                    return GetPath(node, throughStart, throughGoal);

                // Jinak přesuň uzel z otevřených do uzavřených.
                closed.Add(node);
                open.Remove(node);

                // Projdi sousedy
                foreach (Node neighbour in GetNeighbours(node, start, goal, throughObjects, throughClosedDoors, throughImpermeableTerrain, sameLocality, throughStart, throughGoal, maxDistance))
                {
                    // Pokud soused patří mezi uzavřené nebo je na něm nepovolená překážka, přeskoč ho.
                    if (
                        (!Walkable(neighbour, start, goal, throughObjects, throughClosedDoors, throughImpermeableTerrain, sameLocality, throughStart, throughGoal, maxDistance))
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
        private IEnumerable<Node> GetNeighbours(Node parent, Vector2 start, Vector2 goal, bool throughObjects, bool throughClosedDoors, bool throughImpermeableTerrain, bool sameLocality, bool throughStart, bool throughGoal, int maxDistance)
        {
            IEnumerable<Node> neighbours =
                from neighbour in World.Map.GetNeighbours4(parent.Coords)
                select new Node(neighbour.position, parent.Cost + 1, parent);

            foreach (Node n in neighbours)
            {
                n.ComputeDistance(goal);
                if (Walkable(n, start, goal, throughObjects, throughClosedDoors, throughImpermeableTerrain, sameLocality, throughStart, throughGoal, maxDistance))
                    yield return n;
            }
        }
    }
}