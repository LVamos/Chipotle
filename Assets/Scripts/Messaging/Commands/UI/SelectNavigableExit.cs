using Game.Models;

using System.Collections.Generic;

namespace Game.Messaging.Commands.UI
{
	public class SelectNavigableExit : Message
	{
		public readonly List<ExitInfo> Exits;

		public SelectNavigableExit(object sender, List<ExitInfo> exits) : base(sender)
		{
			Exits = exits;
		}
	}
}
