using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands
{
	public class ReportPosition: GameMessage
	{
		public ReportPosition(object sender) : base(sender) { }
	}
}
