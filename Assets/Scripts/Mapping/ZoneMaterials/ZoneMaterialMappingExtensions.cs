using Game.Terrain;

using System.Collections.Generic;
using System.Linq;

using ResonanceMaterial = ResonanceAudioRoomManager.SurfaceMaterial;

namespace Game.Mapping.ZoneMaterials
{
    public static class ZoneMaterialMappingExtensions
    {
        private static readonly Dictionary<ZoneMaterial, ResonanceMaterial> _materials = new()
        {
            { ZoneMaterial.Transparent, ResonanceMaterial.Transparent },
            { ZoneMaterial.AcousticCeilingTiles, ResonanceMaterial.AcousticCeilingTiles },
            { ZoneMaterial.BrickBare, ResonanceMaterial.BrickBare },
            { ZoneMaterial.BrickPainted, ResonanceMaterial.BrickPainted },
            { ZoneMaterial.ConcreteBlockCoarse, ResonanceMaterial.ConcreteBlockCoarse },
            { ZoneMaterial.ConcreteBlockPainted, ResonanceMaterial.ConcreteBlockPainted },
            { ZoneMaterial.CurtainHeavy, ResonanceMaterial.CurtainHeavy },
            { ZoneMaterial.FiberglassInsulation, ResonanceMaterial.FiberglassInsulation },
            { ZoneMaterial.GlassThin, ResonanceMaterial.GlassThin },
            { ZoneMaterial.GlassThick, ResonanceMaterial.GlassThick },
            { ZoneMaterial.Grass, ResonanceMaterial.Grass },
            { ZoneMaterial.LinoleumOnConcrete, ResonanceMaterial.LinoleumOnConcrete },
            { ZoneMaterial.Marble, ResonanceMaterial.Marble },
            { ZoneMaterial.Metal, ResonanceMaterial.Metal },
            { ZoneMaterial.ParquetOnConcrete, ResonanceMaterial.ParquetOnConcrete },
            { ZoneMaterial.PlasterRough, ResonanceMaterial.PlasterRough },
            { ZoneMaterial.PlasterSmooth, ResonanceMaterial.PlasterSmooth },
            { ZoneMaterial.PlywoodPanel, ResonanceMaterial.PlywoodPanel },
            { ZoneMaterial.PolishedConcreteOrTile, ResonanceMaterial.PolishedConcreteOrTile },
            { ZoneMaterial.Sheetrock, ResonanceMaterial.Sheetrock },
            { ZoneMaterial.WaterOrIceSurface, ResonanceMaterial.WaterOrIceSurface },
            { ZoneMaterial.WoodCeiling, ResonanceMaterial.WoodCeiling },
            { ZoneMaterial.WoodPanel, ResonanceMaterial.WoodPanel }
        };

        private static readonly Dictionary<ResonanceMaterial, ZoneMaterial> _reverseMaterials =
            _materials.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static ResonanceMaterial ToResonanceMaterial(this ZoneMaterial value) => _materials[value];

        public static ZoneMaterial ToZoneMaterial(this ResonanceMaterial value) => _reverseMaterials[value];
    }
        }