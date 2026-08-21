using Game.Terrain;

namespace Assets.Scripts.Models
{
	public class ZoneMaterials
	{
		public ZoneMaterial LeftWall;
		public ZoneMaterial FrontWall;
		public ZoneMaterial RightWall;
		public ZoneMaterial BackWall;
		public ZoneMaterial Floor;
		public ZoneMaterial Ceiling;

		public ZoneMaterials() { }

		public ZoneMaterials(ZoneMaterials other)
		{
			LeftWall = other.LeftWall;
			FrontWall = other.FrontWall;
			RightWall = other.RightWall;
			BackWall = other.BackWall;
			Floor = other.Floor;
			Ceiling = other.Ceiling;
		}

		public ZoneMaterials(ZoneMaterial leftwall, ZoneMaterial frontwall, ZoneMaterial rightwall, ZoneMaterial backwall, ZoneMaterial floor, ZoneMaterial ceiling)
		{
			LeftWall = leftwall;
			FrontWall = frontwall;
			RightWall = rightwall;
			BackWall = backwall;
			Floor = floor;
			Ceiling = ceiling;
		}
	}
}
