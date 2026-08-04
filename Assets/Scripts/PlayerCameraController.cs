using Game.Messaging;
using Game.Messaging.Events.Movement;
using Game.UI;

using UnityEngine;

namespace Game
{
	public class PlayerCameraController : MessagingObject
	{
		protected override void HandleMessage(Message message)
		{
			if (message.Sender != World.Player)
				return;

			switch (message)
			{
				case CharacterMoved m: OnCharacterMoved(m); break;
				case CharacterRotated m: OnCharacterRotated(m); break;
				default: base.HandleMessage(message); break;
			}
		}

		private void OnCharacterRotated(CharacterRotated message)
		{
			float source = (float)message.Source.Angle.CartesianDegrees;
			float target = (float)message.Target.Angle.CartesianDegrees;
			float degrees = (float)(source - target);
			degrees = ((degrees + 180f) % 360f + 360f) % 360f - 180f;

			if (degrees != 0)
				StartCoroutine(CameraManager.RotateYaw(degrees, Settings.CameraRotationDuration));
		}

		private void OnCharacterMoved(CharacterMoved message)
		{
			Vector2? oldPosition2d = message.SourcePosition != null ? message.SourcePosition.Value.Center : null;
			Vector2 newPosition2d = message.TargetPosition.Center;
			float y = World.Player.transform.localScale.y;
			Vector3 newPosition3d = message.TargetPosition.Center.ToVector3(y);

			if (oldPosition2d != null
&& World.GetDistance(oldPosition2d.Value, newPosition2d) < 2)
				StartCoroutine(CameraManager.SetPosition(newPosition3d, Settings.CameraRotationDuration));
			else CameraManager.SetPosition(newPosition3d);
		}
	}
}
