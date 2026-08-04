using Game.Terrain;

using System.Collections;

using UnityEngine;

namespace Game.UI
{
	public static class CameraManager
	{
		public static Vector2 Get2dPosition()
		{
			Vector3 position3d = Get3dPosition();
			return new Vector2(position3d.x, position3d.z);
		}

		public static Vector2 GetCameraAllignedPoint(Rectangle area)
		{
			Vector2 position2d = Get2dPosition();

			return area.GetAlignedPoint(position2d)
			?? area.GetClosestPoint(position2d);
		}

		private static Transform Transform => Camera.main.transform;

		public static void SetYaw(float degrees)
			=> Transform.rotation = Quaternion.Euler(0, degrees, 0);

		public static void SetPosition(Vector3 position)
			=> Transform.position = position;

		/// <summary>
		/// Moves the camera to the specified position over time.
		/// </summary>
		/// <param name="position">The target position.</param>
		/// <param name="duration">Duration in seconds for smooth movement.</param>
		/// <returns>IEnumerator for coroutine support.</returns>
		public static IEnumerator SetPosition(Vector3 position, float duration)
		{
			if (duration <= 0f)
			{
				Transform.position = position;
				yield break;
			}

			int steps = 10;
			Vector3 startPosition = Transform.position;
			Vector3 totalMovement = position - startPosition;
			Vector3 stepMovement = totalMovement / steps;
			float stepDuration = duration / steps;
			Vector3 movedSoFar = Vector3.zero;

			for (int i = 0; i < steps - 1; i++)
			{
				Transform.position += stepMovement;
				movedSoFar += stepMovement;
				yield return new WaitForSeconds(stepDuration);
			}

			// Last step to ensure exact final position (compensates for floating point errors)
			Vector3 remainingMovement = totalMovement - movedSoFar;
			Transform.position += remainingMovement;
		}

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
