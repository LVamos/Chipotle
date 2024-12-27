using Assets.Scripts.Terrain;

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
		public void SaveTerrain(XElement locality)
		{
			Rectangle localityArea = new(locality.Attribute("coordinates").Value);
			string localityName = locality.Attribute("indexedname").Value;
			TerrainType defaultTerrain = locality.Attribute("defaultTerrain").Value.ToTerrainType();
			List<XElement> panels = locality.Elements("panel").ToList();

			List<TerrainPanel> savedPanels = new();
			foreach (XElement panel in panels)
			{
				string coords = panel.Attribute("coordinates").Value;
				var panelArea = new Rectangle(coords).ToAbsolute(localityArea);
				TerrainType panelTerrain = panel.Attribute("terrain").Value.ToTerrainType();
				bool permeable = panel.Attribute("canBeOccupied").Value.ToBool();
				savedPanels.Add(new(panelTerrain, permeable, panelArea));
			}

			_terrain[localityName] = savedPanels;
		}

		/// <summary>
		/// Name of an opened map file
		/// </summary>
		public readonly string FileName;

		/// <summary>
		/// Maps tiles to coordinates.
		/// </summary>
		private Dictionary<Vector2, Tile> _cache = new();

		private Dictionary<string, List<TerrainPanel>> _terrain = new(StringComparer.InvariantCultureIgnoreCase);

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="fileName">Name of the map file to be loaded</param>
		public TileMap(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("missing file name");

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
			IEnumerable<Vector2> points = area.GetPoints();
			foreach (Vector2 point in points)
			{
				Tile tile = this[point];

				if (tile == null)
					this[point] = new(terrain, permeable);
				else tile.Register(terrain, permeable);
			}
		}

		/// <summary>
		/// Puts a terrain panel on the map.
		/// </summary>
		/// <param name="panel">A terrain panel defined in an XML node</param>
		/// <param name="localityArea">A locality the terrain panel intersects</param>
		public void DrawTerrain(XElement panel, Rectangle localityArea)
		{
			string coords = panel.Attribute("coordinates").Value;
			Rectangle panelArea = new Rectangle(coords).ToAbsolute(localityArea);
			TerrainType terrain = panel.Attribute("terrain").Value.ToTerrainType();
			bool permeable = panel.Attribute("canBeOccupied").Value.ToBool();
			DrawTerrain(panelArea, terrain, permeable);
		}

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
		{
			Tile tile = null;
			Vector2 rounded = RoundCoordinates(point.x, point.y);
			if (_cache.TryGetValue(rounded, out tile))
				return tile;
			return GetTerrain(point);
		}

		private Tile GetTerrain(Vector2 point)
		{
			Locality locality = World.GetLocality(point);
			if (locality == null)
				return null;

			List<TerrainPanel> panels = _terrain[locality.Name.Indexed];
			for (int i = panels.Count - 1; i >= 0; i--)
			{
				TerrainPanel panel = panels[i];
				if (panel.Area.Intersects(point))
				{
					Tile tile = new(panel.Type, panel.Permeable);
					_cache[point] = tile;
					return tile;
				}
			}

			return new(locality.DefaultTerrain, true);
		}

		/// <summary>
		/// Puts the specified tile on the map on the specified coordinates.
		/// </summary>
		/// <param name="point">Target posiiton of the tile</param>
		/// <param name="tile">The tile to be put</param>
		public void PutTile(Vector2 point, Tile tile)
		{
			_cache[RoundCoordinates(point.x, point.y)] = tile;
		}

		/// <summary>
		/// Rounds the coordinates.
		/// </summary>
		/// <param name="x">The X coordinate</param>
		/// <param name="y">The Y coordinate</param>
		/// <returns>The rounded coordinates</returns>
		private Vector2 RoundCoordinates(float x, float y)
			=> new(x.Round(), y.Round());
	}
}