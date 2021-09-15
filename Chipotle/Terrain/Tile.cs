using System;
using System.Collections.Generic;
using System.Linq;

using Game.Entities;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    /// <summary>
    /// Stores reference to a tile and its position
    /// </summary>
    public class Tile : DebugSO
    {
        /// <summary>
        /// Position on the tile map
        /// </summary>
        public readonly Vector2 Position;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="terrain">Terrain type for this tile</param>
        /// <param name="position">Coordinates of the tile</param>
        /// <param name="locality">Associatet locality</param>
        /// <param name="permeable">
        /// Specifies if an entity or another game object can be placed on this tile
        /// </param>
        /// <param name="passage">Associatet passage</param>
        public Tile(TerrainType terrain, Vector2 position, Locality locality, bool permeable = true, Passage passage = null)
        {
            Terrain = terrain;
            Assert(!position.IsNegative(), "Coordinates cannot be negative");
            Position = position;
            Assert((locality == null && passage == null) || (locality == null ^ passage == null), "locality and passage cann't be both null");
            Locality = locality ?? throw new ArgumentNullException(nameof(locality));
            Permeable = permeable;
            Passage = passage;
        }

        public bool IsOccupied => Object != null;
        public bool IsOnPassage => Passage != null;
        public Locality Locality { get; private set; }
        public GameObject Object { get; private set; }
        public Passage Passage { get; private set; }
        public bool Permeable { get; set; }

        public TerrainType Terrain { get; private set; }

        public bool Walkable => Permeable && !IsOccupied;

        /// <summary>
        /// Returns nearest neighbour tile in given direction.
        /// </summary>
        /// <param name="direction">Direction of wanted neighbour</param>
        /// <returns>Neighbour tile</returns>
        public Tile GetNeighbour(Direction direction)
                    => GetNeighbour(direction.AsVector2());

        /// <summary>
        /// Returns nearest neighbour tile in given direction.
        /// </summary>
        /// <param name="step">Directional vector</param>
        /// <returns>Neighbour tile</returns>
        public Tile GetNeighbour(Vector2 step)
            => World.Map[Position + step];

        public IEnumerable<Tile> GetNeighbours4()
            => DirectionExtension.BasicDirections.Select(d => GetNeighbour(d)).Where(t => t != null);

        /// <summary>
        /// Lists all nearest valid neighbours of the tile
        /// </summary>
        public IEnumerable<Tile> GetNeighbours8()
            => DirectionExtension.DirectionDeltas.Select(d => GetNeighbour(d)).Where(t => t != null && t != this);

        public void Register(TerrainType terrain, bool permeable = true)
        {
            Terrain = terrain;
            Permeable = permeable;
        }

        public void Register(Passage p)
        {
            Passage = !IsOnPassage ? p ?? throw new ArgumentNullException(nameof(p)) : throw new InvalidOperationException(nameof(p));

            if (Terrain == TerrainType.Wall)
                Terrain = Locality.DefaultTerrain;
        }

        public void Register(Locality l)
            => Locality = l ?? throw new ArgumentNullException(nameof(l));

        public void Register(Entity e)
            => Object = Permeable && !IsOccupied ? e ?? throw new ArgumentNullException(nameof(e)) : throw new InvalidOperationException(nameof(e));

        /// <summary>
        /// Puts given game object or entity on the tile.
        /// </summary>
        /// <param name="o">The object or entity to be stored</param>
        public void Register(GameObject o)
            => Object = (Permeable || Terrain == TerrainType.Wall) && !IsOccupied ? (o ?? throw new ArgumentNullException(nameof(o))) : throw new InvalidOperationException(nameof(o));

        public void UnregisterObject()
        {
            Assert(Object != null, "No object here.");
            Object = null;
        }

        public void UnregisterPassage()
        {
            if (Passage != null)
                Passage = null;
            else
                throw new InvalidOperationException("No passage");
        }
    }
}