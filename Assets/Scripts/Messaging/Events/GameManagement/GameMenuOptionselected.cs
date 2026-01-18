using Game.Controls;

namespace Game.Messaging.Events.GameManagement
{
	public class GameMenuOptionselected : Message
	{
		public readonly CommandId Command;

		public GameMenuOptionselected(object sender, CommandId command) : base(sender)
			=> Command = command;
	}
}
