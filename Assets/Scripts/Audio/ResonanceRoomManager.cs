using Assets.Scripts.Models;

using Game.Mapping.ZoneMaterials;
using Game.Terrain;

using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Manages Resonance Audio Room configuration and parameters.
    /// </summary>
    public class ResonanceRoomManager
    {
        private readonly GameObject _roomObject;
        private readonly ResonanceAudioRoom _room;

        public ResonanceRoomManager()
        {
            _roomObject = new("Resonance Audio room");
            _room = _roomObject.AddComponent<ResonanceAudioRoom>();
        }

        public void SetRoomParameters(Zone zone, ZoneMaterials zoneMaterials)
        {
            _roomObject.transform.position = zone.transform.position;
            _room.size = zone.transform.localScale;
            SetMaterials(zoneMaterials);

            if (zone.Type == ZoneType.Outdoor)
                _room.reverbTime = .4f;
        }

        private void SetMaterials(ZoneMaterials zoneMaterials)
        {
            _room.leftWall = zoneMaterials.LeftWall.ToResonanceMaterial();
            _room.frontWall = zoneMaterials.FrontWall.ToResonanceMaterial();
            _room.rightWall = zoneMaterials.RightWall.ToResonanceMaterial();
            _room.backWall = zoneMaterials.BackWall.ToResonanceMaterial();
            _room.floor = zoneMaterials.Floor.ToResonanceMaterial();
            _room.ceiling = zoneMaterials.Ceiling.ToResonanceMaterial();
        }

        public ResonanceAudioRoom Room => _room;
        public GameObject RoomObject => _roomObject;
    }
}
