using Assets.Scripts.Models;

using UnityEngine;

namespace Game.Audio
{
    public class ResonanceRoomParameters
    {
        public ResonanceRoomParameters(
            Vector3 position,
            Vector3 dimensions,
            ZoneMaterials materials,
            bool outdoors)
        {
            Position = position;
            Dimensions = dimensions;
            Materials = materials;
            Outdoors = outdoors;
        }

        public Vector3 Position { get; set; }
        public Vector3 Dimensions { get; set; }
        public ZoneMaterials Materials { get; set; }
        public bool Outdoors { get; set; }
    }
}
