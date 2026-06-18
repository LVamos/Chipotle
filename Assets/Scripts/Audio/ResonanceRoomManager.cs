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
        public void SimulateObstacle(Vector2 obstacleDirection)
        {
            Vector2 playerPosition = World.Player.Center;
            float playerRadius = World.Player.Area.Value.Width * 2;
            Vector3 roomDimensions = World.Player.Zone.transform.localScale;
            obstacleDirection.Normalize();

            float halfWidth = roomDimensions.x * 0.5f;
            float halfHeight = roomDimensions.z * 0.5f;
            Vector2 offset = Vector2.zero;

            // Horizontal wall
            if (Mathf.Abs(obstacleDirection.x) > 0.0001f)
            {
                float horizontalDistance =
                    halfWidth - playerRadius;

                offset.x =
                    -Mathf.Sign(obstacleDirection.x) *
                    horizontalDistance;
            }

            // Vertical wall
            if (Mathf.Abs(obstacleDirection.y) > 0.0001f)
            {
                float verticalDistance =
                    halfHeight - playerRadius;

                offset.y =
                    -Mathf.Sign(obstacleDirection.y) *
                    verticalDistance;
            }

            Vector2 floorCenter = playerPosition + offset;
            _roomObject.transform.position = floorCenter.ToVector3(roomDimensions.y * 0.5f);
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
