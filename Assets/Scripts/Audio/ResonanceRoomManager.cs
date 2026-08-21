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
		private bool _obstacleSimulated;

		public void StopSimulatingObstacle()
		{
			if (!_obstacleSimulated)
				return;

			SimulateRoom(_roomParameters);
			_obstacleSimulated = false;
		}

		public void SimulateObstacle(Vector2 obstacleDirection)
		{
			Vector2 playerPosition = World.Player.Center;
			float playerRadius = World.Player.Area.Value.Width * 2;
			Vector3 roomDimensions = World.Player.Zone.transform.localScale;
			obstacleDirection.Normalize();

			Vector2 roomHalfSize = new(roomDimensions.x * 0.5f, roomDimensions.z * 0.5f);
			Vector2 directionSign = new(Mathf.Sign(obstacleDirection.x), Mathf.Sign(obstacleDirection.y));
			Vector2 roomOffset = -Vector2.Scale(directionSign, roomHalfSize - Vector2.one * playerRadius);

			Vector2 roomCenter = playerPosition + roomOffset;
			_roomObject.transform.position = roomCenter.ToVector3(roomDimensions.y * 0.5f);
			_obstacleSimulated = true;
		}

		private readonly GameObject _roomObject;
		private readonly ResonanceAudioRoom _room;

		public ResonanceRoomManager()
		{
			_roomObject = new("Resonance Audio room");
			_room = _roomObject.AddComponent<ResonanceAudioRoom>();
		}

		private ResonanceRoomParameters _roomParameters;

		public void SimulateRoom(ResonanceRoomParameters parameters)
		{
			_roomParameters = parameters;
			_roomObject.transform.position = parameters.Position;
			_room.size = parameters.Dimensions;
			SetMaterials(parameters.Materials);

			_room.reverbTime = parameters.Outdoors ? .4f : 1;
		}

		public void EnableReflectivity()
			=> _room.reflectivity = 1;

		public void DisableReflectivity()
			=> _room.reflectivity = 0;

		public void SetMaterials(ZoneMaterials zoneMaterials)
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
