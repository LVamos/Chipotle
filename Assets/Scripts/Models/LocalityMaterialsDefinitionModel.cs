using Assets.Scripts.Terrain;

namespace Assets.Scripts.Models
{
    public class ZoneMaterialsDefinitionModel
    {
        public Material LeftWall;
        public Material FrontWall;
        public Material RightWall;
        public Material BackWall;
        public Material Floor;
        public Material Ceiling;

        public ZoneMaterialsDefinitionModel() { }

        public ZoneMaterialsDefinitionModel(Material leftwall, Material frontwall, Material rightwall, Material backwall, Material floor, Material ceiling)
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
