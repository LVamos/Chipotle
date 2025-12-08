using Game.Messaging;
using Game.Terrain;

namespace Assets.Scripts.Messaging.Commands.Characters
{
	public class NavigateToExit : Message
	{
		public readonly Passage Exit;

		public NavigateToExit(object sender, Passage exit) : base(sender)
		{
			Exit = exit;
		}
	}
}
