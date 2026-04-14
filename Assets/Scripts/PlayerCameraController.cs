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
			if (source == target)
			{
				CameraManager.SetYaw(target);
				return;
			}

			float degrees = (float)(source - target);
			CameraManager.RotateYaw(degrees);
		}

		private void OnCharacterMoved(CharacterMoved message)
		{
			float y = World.Player.transform.localScale.y;
			Vector3 position = message.TargetPosition.Center.ToVector3(y);
			CameraManager.SetPosition(position);
		}
	}
}
