using Game.Controls;

namespace Game.Messaging.Events.GameActions
{
	public class GameMenuOptionselected : Message
	{
		public readonly CommandId Command;

		public GameMenuOptionselected(object sender, CommandId command) : base(sender)
			=> Command = command;
	}
}
