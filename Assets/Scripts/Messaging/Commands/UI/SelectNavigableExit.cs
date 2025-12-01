using Game.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands.UI
{
	public class SelectNavigableExit:Message
	{
		public readonly List<ExitInfo> Exits;

		public SelectNavigableExit(object sender,List<ExitInfo> exits):base(sender)
		{
			Exits = exits;
		}
	}
}
