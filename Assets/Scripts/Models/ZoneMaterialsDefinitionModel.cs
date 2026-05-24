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
