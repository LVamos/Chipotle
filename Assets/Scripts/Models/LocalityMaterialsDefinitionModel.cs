using Assets.Scripts.Terrain;

namespace Assets.Scripts.Models
{
    public class LocalityMaterialsDefinitionModel
    {
        public Material LeftWall;
        public Material FrontWall;
        public Material RightWall;
        public Material BackWall;
        public Material Floor;
        public Material Ceiling;

        public LocalityMaterialsDefinitionModel() { }

        public LocalityMaterialsDefinitionModel(Material leftwall, Material frontwall, Material rightwall, Material backwall, Material floor, Material ceiling)
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
