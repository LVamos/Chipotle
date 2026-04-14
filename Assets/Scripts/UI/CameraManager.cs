using UnityEngine;

namespace Game.UI
{
	public static class CameraManager
	{
		private static Transform Transform => Camera.main.transform;

		public static void SetYaw(float degrees)
			=> Transform.rotation = Quaternion.Euler(0, degrees, 0);

		public static void SetPosition(Vector3 position)
			=> Transform.position = position;

		public static Vector3 Get3dPosition()
			=> Transform.position;

		public static void InitYaw()
			=> Transform.rotation = Quaternion.identity;

		public static void RotateYaw(float degrees)
			=> Transform.Rotate(0, degrees, 0);
	}
}
