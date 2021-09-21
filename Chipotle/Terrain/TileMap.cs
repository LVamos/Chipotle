using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a dictionary-based tile map.
    /// </summary>
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
        public void DrawTerrain(Plane area, TerrainType terrain, bool permeable, Locality locality)
        {
            foreach (Vector2 point in area.GetPoints())
            {
                Tile tile = this[point];

                if (tile == null)
                    this[point] = new Tile(terrain, point, locality, permeable);
                else
                {
                    tile.Register(terrain);
                    tile.Permeable = permeable;
                    tile.Register(locality);
                }
            }
        }

        /// <summary>
        /// Puts a terrain panel on the map.
        /// </summary>
        /// <param name="xPanel">A terrain panel defined in an XML node</param>
        /// <param name="locality">A loclaity the terrain panel intersects</param>
        public void DrawTerrain(XElement xPanel, Locality locality)
                    => DrawTerrain(new Plane(xPanel.Attribute("coordinates").Value).ToAbsolute(locality), xPanel.Attribute("terrain").Value.ToTerrainType(), xPanel.Attribute("canBeOccupied").Value.ToBool(), locality);

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