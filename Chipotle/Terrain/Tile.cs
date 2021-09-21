using System;
using System.Collections.Generic;
using System.Linq;

using Game.Entities;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    /// <summary>
    /// Represents one square tile on the game map.
    /// </summary>
    public class Tile : DebugSO
    {
        /// <summary>
        /// Position of the tile on the game map
        /// </summary>
        public readonly Vector2 Position;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="terrain">Terrain type on this tile</param>
        /// <param name="position">Position of the tile on the tile map</param>
        /// <param name="locality">A locality intersecting the tile</param>
        /// <param name="permeable">
        /// Specifies if an entity or another game object can be placed on this tile
        /// </param>
        /// <param name="passage">A passage intersecting the tile</param>
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

        /// <summary>
        /// Indicates if the tile is occupied by an NPC or game object.
        /// </summary>
        public bool IsOccupied
            => Object != null;

        /// <summary>
        /// Indicates if the tile intersects a passage.
        /// </summary>
        public bool IsOnPassage
            => Passage != null;

        /// <summary>
        /// Returns a locality the tile intersects.
        /// </summary>
        public Locality Locality { get; private set; }

        /// <summary>
        /// Returns an NPC or game object the tile intersects.
        /// </summary>
        public GameObject Object { get; private set; }

        /// <summary>
        /// Returns an passage the tile intersects.
        /// </summary>
        public Passage Passage { get; private set; }

        /// <summary>
        /// Specifies if the tile is walkable
        /// </summary>
        public bool Permeable { get; set; }

        /// <summary>
        /// type of the terrain laying on this tile
        /// </summary>
        public TerrainType Terrain { get; private set; }

        /// <summary>
        /// Checks if the tile is walkable for NPCs.
        /// </summary>
        public bool Walkable
            => Permeable && !IsOccupied;

        /// <summary>
        /// Returns an adjacent tile in the specified direction.
        /// </summary>
        /// <param name="direction">Direction of wanted neighbour</param>
        /// <returns>An adjacent tile</returns>
        public Tile GetNeighbour(Direction direction)
                    => GetNeighbour(direction.AsVector2());

        /// <summary>
        /// Returns adjacent tile in the specified direction.
        /// </summary>
        /// <param name="direction">A directional unit vector</param>
        /// <returns>An adjacent tile</returns>
        public Tile GetNeighbour(Vector2 direction)
            => World.Map[Position + direction];

        /// <summary>
        /// Enumerates adjacent tiles in four basic directions.
        /// </summary>
        /// <returns>The adjacent tiles in four basic directions</returns>
        public IEnumerable<Tile> GetNeighbours4()
            => DirectionExtension.BasicDirections.Select(d => GetNeighbour(d))
            .Where(t => t != null);

        /// <summary>
        /// Lists all adjacent tiles
        /// </summary>
        public IEnumerable<Tile> GetNeighbours8()
            => DirectionExtension.DirectionDeltas.Select(d => GetNeighbour(d)).Where(t => t != null && t != this);

        /// <summary>
        /// Puts terrain on this tile.
        /// </summary>
        /// <param name="terrain">Type of the terrain</param>
        /// <param name="permeable">Specifies if the tile is accessible for NPCs</param>
        public void Register(TerrainType terrain, bool permeable = true)
        {
            Terrain = terrain;
            Permeable = permeable;
        }

        /// <summary>
        /// Puts a passage on the tile.
        /// </summary>
        /// <param name="p">The passage to be registered</param>
        public void Register(Passage p)
        {
            Passage = !IsOnPassage ? p ?? throw new ArgumentNullException(nameof(p)) : throw new InvalidOperationException(nameof(p));

            if (Terrain == TerrainType.Wall)
                Terrain = Locality.DefaultTerrain;
        }

        /// <summary>
        /// Puts a locality on this tile.
        /// </summary>
        /// <param name="l">The locality to be registered</param>
        public void Register(Locality l)
            => Locality = l ?? throw new ArgumentNullException(nameof(l));

        /// <summary>
        /// Puts an NPC on this tile.
        /// </summary>
        /// <param name="e">The entity to be registered</param>
        public void Register(Entity e)
            => Object = Permeable && !IsOccupied ? e ?? throw new ArgumentNullException(nameof(e)) : throw new InvalidOperationException(nameof(e));

        /// <summary>
        /// Puts a game object on the tile.
        /// </summary>
        /// <param name="o">The object to be registered</param>
        public void Register(GameObject o)
            => Object = (Permeable || Terrain == TerrainType.Wall) && !IsOccupied ? (o ?? throw new ArgumentNullException(nameof(o))) : throw new InvalidOperationException(nameof(o));

        /// <summary>
        /// Removes a game object from the tile if there's any.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void UnregisterObject()
        {
            Assert(Object != null, "No object here.");
            Object = null;
        }

        /// <summary>
        /// Removes passge from the tile.
        /// </summary>
        public void UnregisterPassage()
        {
            Assert(Passage != null, "No passage");
            Passage = null;
        }
    }
}