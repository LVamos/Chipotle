using Assets.Scripts.Terrain;

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
				Rectangle panelArea = new Rectangle(coords).ToAbsolute(localityArea);
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
		/// Returns an adjacent tile in the specified direction.
		/// </summary>
		/// <param name="position">Position of the concerning tile</param>
		/// <param name="direction">Direction of wanted neighbour</param>
		/// <returns>An adjacent tile and its position in a tuple</returns>
		public TileInfo GetNeighbour(Vector2 position, Direction direction)
		{
			Vector2 vector = direction.AsVector2();
			Vector2 finalVector = new(vector.x * .1f, vector.y * .1f);
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
		public IEnumerable<TileInfo> GetNeighbours8(Vector2 position)
		{
			return DirectionExtension.DirectionDeltas.Select(d => GetNeighbour(position, d)).Where(t => t.Tile != null && t.Position != position);
		}

		/// <summary>
		/// Returns a tile on the specified position.
		/// </summary>
		/// <param name="point">Position of the required tile</param>
		/// <returns>a tile on the specified position</returns>
		public Tile GetTile(Vector2 point)
		{
			Tile tile = null;
			Vector2 rounded = RoundCoordinates(point.x, point.y);
			return _cache.TryGetValue(rounded, out tile) ? tile : GetTerrain(point);
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
					Tile tile = new(panel.Type, panel.Permeable, locality);
					_cache[point] = tile;
					return tile;
				}
			}

			return new(locality.DefaultTerrain, true, locality);
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
		{
			return new(x.Round(), y.Round());
		}
	}
}