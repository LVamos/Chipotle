using Game.Controls;

using System.Collections.Generic;
using System.ComponentModel.Design;

namespace Game.Messaging.Commands.UI
{
	public class OpenGameMenu : Message
	{
		public readonly List<CommandId> Commands;

		public OpenGameMenu(object sender, List<CommandId> commands) : base(sender)
		{
			Commands = commands;
		}
	}
}
