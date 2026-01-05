using Game.Models;

namespace Game.Messaging.Commands.Characters
{
	public class CharacterHitDoor : Message
	{
		public readonly NavigableExitInfo Exit;

		public CharacterHitDoor(object sender, NavigableExitInfo exit) : base(sender)
		{
			Exit = exit;
		}
	}
}
