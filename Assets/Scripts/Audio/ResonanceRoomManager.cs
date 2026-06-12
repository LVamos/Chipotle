using Assets.Scripts.Models;

using Game.Mapping.ZoneMaterials;

using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Manages Resonance Audio Room configuration and parameters.
    /// </summary>
    public class ResonanceRoomManager
    {
        public void SimulateObstacle()
        {

        }

        public void SimulateTightRoom(float height, float ceiling)
        {
            Vector3 playerPosition = World.Player.transform.position;
            _roomObject.transform.position = new Vector3(playerPosition.x, height, playerPosition.z);
            Vector3 size = new(4, ceiling, 4);
            _room.size = size;
        }

        private readonly GameObject _roomObject;
        private readonly ResonanceAudioRoom _room;

        public ResonanceRoomManager()
        {
            _roomObject = new("Resonance Audio room");
            _room = _roomObject.AddComponent<ResonanceAudioRoom>();
        }

        public void SimulateRoom
            (
            Vector3 position,
            Vector3 dimensions,
            ZoneMaterials materials,
            bool outdoors
            )
        {
            _roomObject.transform.position = position;
            _room.size = dimensions;
            SetMaterials(materials);

            _room.reverbTime = outdoors ? .4f : 1;
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
