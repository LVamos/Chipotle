using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands.GameInfo
{
	public class SayNavigatedObjectLocation : Message
	{
		public SayNavigatedObjectLocation(object sender) : base(sender)
		{ }
	}
}
