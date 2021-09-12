using Game.Entities;

using Luky;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// Generic base class for storing a tile map
    /// </summary>
    public class TileMap : DebugSO
    {
        public readonly string FileName;

        public TileMap(string fileName = null)
            => FileName = fileName;

        /// <summary>
        /// Finds last tile in given direction.
        /// </summary>
        /// <param name="start">The default coordinates</param>
        /// <param name="direction">Direction of map edge</param>
        /// <returns>Coordinates of last tile in given direction</returns>
        public Vector2 GetLastTile(Vector2 start, Direction direction)
        {
            Vector2 point;

            switch (direction)
            {
                case Direction.Left: point = new Vector2(LeftBorder, start.Y); break;
                case Direction.Up: point = new Vector2(start.X, TopBorder); break;
                case Direction.Right: point = new Vector2(RightBorder, start.Y); break;
                case Direction.Down: point = new Vector2(start.X, BottomBorder); break;
                default: throw new ArgumentException(nameof(direction));
            }

            while (World.Map[point] == null)
                point += direction.GetOpposite().AsVector2();

            return point;
        }



        /// <summary>
        /// Checks if coordinates point on map edge.
        /// </summary>
        /// <param name="point">The coordinates to check</param>
        /// <returns>True if coordinates point on edge</returns>
        public bool IsOnEdge(Vector2 point)
            => point.X == LeftBorder || point.X == RightBorder || point.Y == BottomBorder || point.Y == TopBorder;


        //todo TileMap<T>: zrevidovat



        /// <summary>
        /// Enumerates all coordinates from start to map edge in given direction.
        /// </summary>
        /// <param name="start">The beginning</param>
        /// <param name="direction">Direction of map edge</param>
        /// <returns>Collection of coordinates</returns>
        public IEnumerable<Vector2> GetPointsToEdge(Vector2 start, Direction direction)
        {
            Vector2 step = direction.AsVector2();

            for (Vector2 point = start; IsInBoundaries(point); point += step)
                yield return point;
        }

        public bool IsInBoundaries(Vector2 point)
=> point.X >= LeftBorder && point.X <= RightBorder && point.Y >= BottomBorder && point.Y <= TopBorder;



        /// <summary>
        /// Backing array containing the tiles
        /// </summary>
        private Dictionary<Vector2, Tile> _tiles = new Dictionary<Vector2, Tile>();
        private float _bottomBorder;
        private float _leftBorder;
        private float _rightBorder;
        private float _topBorder;

        /// <summary>
        /// Count of all rows in the map
        /// </summary>
        public float TopBorder { get => _topBorder; private set => _topBorder = value; }

        /// <summary>
        /// Count of all columns of the map
        /// </summary>
        public float RightBorder { get => _rightBorder; private set => _rightBorder = value; }

        public float LeftBorder { get => _leftBorder; private set => _leftBorder = value; }
        public float BottomBorder { get => _bottomBorder; private set => _bottomBorder = value; }















        public Tile GetTile(float x, float y)
            => GetTile(new Vector2(x, y));

        public Tile GetTile(Vector2 point)
=> _tiles.TryGetValue(RoundCoordinates(point.X, point.Y), out Tile t) ? t : null;

        private Vector2 RoundCoordinates(float x, float y)
            => new Vector2((float)Math.Round(x), (float)Math.Round(y));


        public void PutTile(float x, float y, Tile tile)
            => PutTile(new Vector2(x, y), tile);

        public void PutTile(Vector2 point, Tile tile)
        {
            _tiles[RoundCoordinates(point.X, point.Y)] = tile;
            UpdateBorders(point);
        }

        private void UpdateBorders(Vector2 point)
        {
            if (LeftBorder > point.X)
                LeftBorder = point.X;

            if (RightBorder < point.X)
                RightBorder = point.X;

            if (TopBorder < point.Y)
                TopBorder = point.Y;

            if (BottomBorder > point.Y)
                BottomBorder = point.Y;
        }

        /// <summary>
        /// Indexer for easier acces to individual tiles
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Structure with information about one tile</returns>
        public Tile this[Vector2 point]
        {
            get => GetTile(point); //_tiles[(int)position.X - LeftBorder, (int)position.Y - BottomBorder];
            set => PutTile(point, value); // _tiles[(int)position.X - LeftBorder, (int)position.Y - BottomBorder]=value;
        }

        public Tile this[float x, float y] { get => GetTile(x, y); set => PutTile(x, y, value); }

        /// <summary>
        /// Enumerates all tiles from start to edge of the map in given direction.
        /// </summary>
        /// <param name="start">Start coordinates</param>
        /// <param name="direction">Edge of the map</param>
        /// <returns>Enumeratiion of tiles</returns>
        public IEnumerable<Tile> GetTilesToEdge(Vector2 start, Direction direction)
            => GetPointsToEdge(start, direction).Where(p => _tiles.ContainsKey(p)).Select(p => this[p]);

        /// <summary>
        /// Saves an area of terrain into the map.
        /// </summary>
        /// <param name="area">Coordinates of drawn area</param>
        /// <param name="terrain">Type of the terrain to draw</param>
        /// <param name="permeable">Specifies if the terrain should be accessible for objects and entities/// <param name="locality"></param>
        public void DrawTerrain(Plane area, TerrainType terrain, bool permeable, Locality locality)
        {
            Assert(!area.IsNegative(), "Invalid coordinates");

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

        public void DrawTerrain(Plane area, GameObject gameObject)
        {
            Assert(!area.IsNegative(), "Invalid coordinates");

            area.GetTiles().Foreach(t => t.Register(gameObject));

        }

        /// <summary>
        /// Sets tiles in given area to null.
        /// </summary>
        /// <param name="area">Co-ordinates of the area to be erased</param>
		public void Erase(Plane area)
=> area.GetPoints().Foreach(p => this[p] = null);

        public void DrawTerrain(Plane area, TerrainType terrain, bool permeable)
        {
            Assert(area.IsInOneLocality(), "Terrain can be drawn only inside one locality at once.");
            area.GetTiles().Foreach(t => t.Register(terrain, permeable));
        }

        public void DrawTerrain(XElement xPanel, Locality locality)
            => DrawTerrain(new Plane(xPanel.Attribute("coordinates").Value).ToAbsolute(locality), xPanel.Attribute("terrain").Value.ToTerrainType(), xPanel.Attribute("canBeOccupied").Value.ToBool(), locality);

    }
}
