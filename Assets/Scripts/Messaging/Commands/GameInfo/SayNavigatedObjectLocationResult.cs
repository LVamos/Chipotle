using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands.GameInfo
{
	public class SayNavigatedObjectLocationResult:Message
	{
		public bool NoNavigatedObjects;

		public SayNavigatedObjectLocationResult(object sender, bool noNavigatedObjects=true): base(sender)
		{
			NoNavigatedObjects = noNavigatedObjects;
		}
	}
}
