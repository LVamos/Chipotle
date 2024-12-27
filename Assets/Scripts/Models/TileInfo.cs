using Game.Terrain;

using UnityEngine;

namespace Game.Models
{
	public class TileInfo
	{
		public Vector2 Position;
		public Tile Tile;

		public TileInfo(Vector2 position, Tile tile)
		{
			Position = position;
			Tile = tile;
		}
	}
}