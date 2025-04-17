using Game.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using UnityEngine;

namespace Game.Terrain
{
	/// <summary>
	/// Represents a dictionary-based tile map.
	/// </summary>
	[Serializable]
	public class TileMap
	{
		public Vector2 SnapToGrid(Vector2 point)
		{
			float x = TileSize * Mathf.Round(point.x / TileSize);
			float y = TileSize * Mathf.Round(point.y / TileSize);

			x = Mathf.Round(x * 10f) / 10f;
			y = Mathf.Round(y * 10f) / 10f;

			return new Vector2(x, y);
		}

		public readonly float TileSize = .1f;

		/// <summary>
		/// Name of an opened map file
		/// </summary>
		public readonly string FileName;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fileName">Name of the map file to be loaded</param>
		public TileMap(string fileName, int tileCount)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("missing file name");

			FileName = fileName;
			_terrain = new Dictionary<Vector2, Tile>(tileCount);
		}

		/// <summary>
		/// An indexer for individual tiles
		/// </summary>
		/// <param name="point">Coordinates of the required tile</param>
		/// <returns>The required tile</returns>
		public Tile this[Vector2 point]
		{
			get => GetTile(point); //_tiles[(int)position.X - LeftBorder, (int)position.Y - BottomBorder];
			set => PutTile(point, value);
		}

		/// <summary>
		/// Returns an adjacent tile in the specified direction.
		/// </summary>
		/// <param name="position">Position of the concerning tile</param>
		/// <param name="direction">Direction of wanted neighbour</param>
		/// <returns>An adjacent tile and its position in a tuple</returns>
		public TileInfo GetNeighbour(Vector2 position, Direction direction)
		{
			Vector2 vector = direction.AsVector2();
			Vector2 finalVector = new(vector.x * TileSize, vector.y * TileSize);
			return GetNeighbour(position, finalVector);
		}

		/// <summary>
		/// Returns adjacent tile in the specified direction.
		/// </summary>
		/// <param name="position">Position of the concerning tile</param>
		/// <param name="direction">A directional unit vector</param>
		/// <returns>An adjacent tile and its position in a tuple</returns>
		public TileInfo GetNeighbour(Vector2 position, Vector2 direction)
		{
			Vector2 finalPosition = position + direction;
			finalPosition = RoundCoordinates(finalPosition.x, finalPosition.y);
			Tile tile = World.Map[finalPosition];
			return new(finalPosition, tile);
		}

		/// <summary>
		/// Enumerates adjacent tiles in four basic directions.
		/// </summary>
		/// <param name="position">Position of the concerning tile</param>
		/// <returns>The adjacent tiles in four basic directions</returns>
		public IEnumerable<TileInfo> GetNeighbours4(Vector2 position)
		{
			return
				from direction in DirectionExtension.BasicDirections
				let neighbour = GetNeighbour(position, direction)
				where neighbour.Tile != null
				select neighbour;
		}

		/// <summary>
		/// Lists all adjacent tiles
		/// </summary>
		/// <param name="position">Position of the concerning tile</param>
		public IEnumerable<TileInfo> GetNeighbours8(Vector2 position) => DirectionExtension.DirectionDeltas.Select(d => GetNeighbour(position, d)).Where(t => t.Tile != null && t.Position != position);

		/// <summary>
		/// Returns a tile on the specified position.
		/// </summary>
		/// <param name="point">Position of the required tile</param>
		/// <returns>a tile on the specified position</returns>
		public Tile GetTile(Vector2 point)
		{
			Vector2 rounded = SnapToGrid(point);
			_terrain.TryGetValue(rounded, out Tile tile);
			return tile;
		}

		public void DrawZone(XElement zoneNode)
		{
			Rectangle zoneArea = new(zoneNode.Attribute("coordinates").Value);
			TerrainType defaultTerrain = zoneNode.Attribute("defaultTerrain").Value.ToTerrainType();

			DrawTerrain(zoneArea, defaultTerrain);
			foreach (XElement panel in zoneNode.Elements("panel"))
				DrawPanel(panel, zoneArea);
		}

		private void DrawTerrain(Rectangle area, TerrainType terrain, bool permeable = true)
		{
			Vector2[] points = area.GetPoints(TileSize).ToArray();
			foreach (Vector2 point in points)
			{
				_terrain[point] = new(terrain, permeable, null);
			}
		}

		private void DrawPanel(XElement panel, Rectangle zoneArea)
		{
			string coords = panel.Attribute("coordinates").Value;
			Rectangle panelArea = new Rectangle(coords).ToAbsolute(zoneArea);
			TerrainType panelTerrain = panel.Attribute("terrain").Value.ToTerrainType();
			bool permeable = panel.Attribute("canBeOccupied").Value.ToBool();

			foreach (Vector2 point in panelArea.GetPoints(TileSize))
				_terrain[point] = new(panelTerrain, permeable, null);

		}

		private Dictionary<Vector2, Tile> _terrain;

		/// <summary>
		/// Puts the specified tile on the map on the specified coordinates.
		/// </summary>
		/// <param name="point">Target posiiton of the tile</param>
		/// <param name="tile">The tile to be put</param>
		public void PutTile(Vector2 point, Tile tile)
		{
			//test
			if (point == new Vector2(1031.7f, 1035.5f))
				System.Diagnostics.Debugger.Break();

			_terrain[SnapToGrid(point)] = tile;
		}

		/// <summary>
		/// Rounds the coordinates.
		/// </summary>
		/// <param name="x">The X coordinate</param>
		/// <param name="y">The Y coordinate</param>
		/// <returns>The rounded coordinates</returns>
		private Vector2 RoundCoordinates(float x, float y) => new(x.Round(), y.Round());

		public void RegisterZone(Zone zone)
		{
			foreach (Vector2 point in zone.Area.Value.GetPoints(TileSize))
				_terrain[point].AddZone(zone);
		}
	}
}