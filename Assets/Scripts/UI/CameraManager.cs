using System.Collections;
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

		/// <summary>
		/// Rotates the camera yaw by the specified degrees instantly.
		/// </summary>
		/// <param name="degrees">The rotation amount in degrees.</param>
		public static void RotateYaw(float degrees)
			=> Transform.Rotate(0, degrees, 0);

		/// <summary>
		/// Rotates the camera yaw by the specified degrees over time.
		/// </summary>
		/// <param name="degrees">The rotation amount in degrees.</param>
		/// <param name="duration">Duration in seconds for smooth rotation.</param>
		/// <returns>IEnumerator for coroutine support.</returns>
		public static IEnumerator RotateYaw(float degrees, float duration)
		{
			if (duration <= 0f)
			{
				Transform.Rotate(0, degrees, 0);
				yield break;
			}

			int steps = 10;
			float stepDegrees = degrees / steps;
			float stepDuration = duration / steps;
			float rotatedSoFar = 0f;

			for (int i = 0; i < steps - 1; i++)
			{
				Transform.Rotate(0, stepDegrees, 0);
				rotatedSoFar += stepDegrees;
				yield return new WaitForSeconds(stepDuration);
			}

			// Last step to ensure exact final rotation (compensates for floating point errors)
			float remainingDegrees = degrees - rotatedSoFar;
			Transform.Rotate(0, remainingDegrees, 0);
		}

		public static void SetOrientation(Vector3
			orientation)
			=> Transform.rotation = Quaternion.Euler(orientation);

		public static Vector3 GetOrientation()
			=> Transform.rotation.eulerAngles;
	}
}
