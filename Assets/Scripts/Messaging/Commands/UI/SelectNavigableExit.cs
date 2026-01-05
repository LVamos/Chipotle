using Game.Models;

using System.Collections.Generic;

namespace Game.Messaging.Commands.UI
{
	public class SelectNavigableExit : Message
	{
		public readonly List<NavigableExitInfo> Exits;

		public SelectNavigableExit(object sender, List<NavigableExitInfo> exits) : base(sender)
		{
			Exits = exits;
		}
	}
}
