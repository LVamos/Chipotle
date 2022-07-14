using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands
{
	public class CreatePredefinedSave: GameMessage
	{
		public CreatePredefinedSave(object sender):base(sender) { }
	}
}
