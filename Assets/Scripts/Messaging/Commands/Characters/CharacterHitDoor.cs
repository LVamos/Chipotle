using Game.Models;

namespace Game.Messaging.Commands.Characters
{
	public class CharacterHitDoor : Message
	{
		public readonly ExitInfo Exit;

		public CharacterHitDoor(object sender, ExitInfo exit) : base(sender)
		{
			Exit = exit;
		}
	}
}
