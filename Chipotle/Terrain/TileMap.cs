using Luky;

using OpenTK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a dictionary-based tile map.
    /// </summary>
    [Serializable]
    public class TileMap : DebugSO
    {
        /// <summary>
        /// Name of an opened map file
        /// </summary>
        public readonly string FileName;

        /// <summary>
        /// Maps tiles to coordinates.
        /// </summary>
        private readonly Dictionary<Vector2, Tile> _tiles = new Dictionary<Vector2, Tile>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">Name of the map file to be loaded</param>
        public TileMap(string fileName)
        {
            Assert(!string.IsNullOrEmpty(fileName), "missing file name");
            FileName = fileName;
        }

        /// <summary>
        /// An indexer for individual tiles
        /// </summary>
        /// <param name="point">Coordinates of the required tile</param>
        /// <returns>The required tile</returns>
        public Tile this[Vector2 point]
        {
            get => GetTile(point); //_tiles[(int)position.X - LeftBorder, (int)position.Y - BottomBorder];
            set => PutTile(point, value); // _tiles[(int)position.X - LeftBorder, (int)position.Y - BottomBorder]=value;
        }

        /// <summary>
        /// Puts terrain on the specified area.
        /// </summary>
        /// <param name="area">Coordinates of the area to be occupied</param>
        /// <param name="terrain">Type of the terrain to be put</param>
        /// <param name="permeable">
        /// Specifies if the terrain should be accessible to NPCs and game objects.
        /// </param>
        /// <param name="locality">A locality to be registered</param>
        public void DrawTerrain(Rectangle area, TerrainType terrain, bool permeable)
        {
            foreach (Vector2 point in area.GetPoints())
            {
                Tile tile = this[point];

                if (tile == null)
                    this[point] = new Tile(terrain, permeable);
                else
                {
                    tile.Register(terrain, permeable);
                }
            }
        }

        /// <summary>
        /// Puts a terrain panel on the map.
        /// </summary>
        /// <param name="xPanel">A terrain panel defined in an XML node</param>
        /// <param name="area">A loclaity the terrain panel intersects</param>
        public void DrawTerrain(XElement xPanel, Rectangle area)
                    => DrawTerrain(new Rectangle(xPanel.Attribute("coordinates").Value).ToAbsolute(area), xPanel.Attribute("terrain").Value.ToTerrainType(), xPanel.Attribute("canBeOccupied").Value.ToBool());

        /// <summary>
        /// Returns an adjacent tile in the specified direction.
        /// </summary>
        /// <param name="position">Position of the concerning tile</param>
        /// <param name="direction">Direction of wanted neighbour</param>
        /// <returns>An adjacent tile and its position in a tuple</returns>
        public (Vector2 position, Tile tile) GetNeighbour(Vector2 position, Direction direction)
                    => GetNeighbour(position, direction.AsVector2());

        /// <summary>
        /// Returns adjacent tile in the specified direction.
        /// </summary>
        /// <param name="position">Position of the concerning tile</param>
        /// <param name="direction">A directional unit vector</param>
        /// <returns>An adjacent tile and its position in a tuple</returns>
        public (Vector2 position, Tile tile) GetNeighbour(Vector2 position, Vector2 direction)
            => (position + direction, World.Map[position + direction]);

        /// <summary>
        /// Enumerates adjacent tiles in four basic directions.
        /// </summary>
        /// <param name="position">Position of the concerning tile</param>
        /// <returns>The adjacent tiles in four basic directions</returns>
        public IEnumerable<(Vector2 position, Tile tile)> GetNeighbours4(Vector2 position)
            => DirectionExtension.BasicDirections.Select(d => GetNeighbour(position, d))
            .Where(t => t.tile != null);

        /// <summary>
        /// Lists all adjacent tiles
        /// </summary>
        /// <param name="position">Position of the concerning tile</param>
        public IEnumerable<(Vector2 position, Tile tile)> GetNeighbours8(Vector2 position)
            => DirectionExtension.DirectionDeltas.Select(d => GetNeighbour(position, d)).Where(t => t.tile != null && t.position != position);

        /// <summary>
        /// Returns a tile on the specified position.
        /// </summary>
        /// <param name="point">Position of the required tile</param>
        /// <returns>a tile on the specified position</returns>
        public Tile GetTile(Vector2 point)
        => _tiles.TryGetValue(RoundCoordinates(point.X, point.Y), out Tile t) ? t : null;

        /// <summary>
        /// Puts the specified tile on the map on the specified coordinates.
        /// </summary>
        /// <param name="point">Target posiiton of the tile</param>
        /// <param name="tile">The tile to be put</param>
        public void PutTile(Vector2 point, Tile tile)
            => _tiles[RoundCoordinates(point.X, point.Y)] = tile;

        /// <summary>
        /// Rounds the coordinates.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <returns>The rounded coordinates</returns>
        private Vector2 RoundCoordinates(float x, float y)
                            => new Vector2((float)Math.Round(x), (float)Math.Round(y));
    }
}