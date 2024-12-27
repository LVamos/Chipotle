using Game.Terrain;

namespace Assets.Scripts.Terrain
{
	public class TerrainPanel
	{
		public TerrainType Type;
		public bool Permeable;
		public Rectangle Area;

		public TerrainPanel(TerrainType type, bool permeable, Rectangle area)
		{
			Type = type;
			Permeable = permeable;
			Area = area;
		}
	}
}
