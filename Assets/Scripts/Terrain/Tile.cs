using System;

namespace Game.Terrain
{
	/// <summary>
	/// Represents one square tile on the game map.
	/// </summary>
	[Serializable]
	public class Tile
	{
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
		public Tile(TerrainType terrain, bool permeable = true)
		{
			Terrain = terrain;
			Walkable = permeable;
		}

		/// <summary>
		/// Specifies if the tile is walkable
		/// </summary>
		public bool Walkable { get; private set; }

		/// <summary>
		/// type of the terrain laying on this tile
		/// </summary>
		public TerrainType Terrain { get; private set; }

		/// <summary>
		/// Puts terrain on this tile.
		/// </summary>
		/// <param name="terrain">Type of the terrain</param>
		/// <param name="permeable">Specifies if the tile is accessible for NPCs</param>
		public void Register(TerrainType terrain, bool permeable = true)
		{
			Terrain = terrain;
			Walkable = permeable;
		}
	}
}