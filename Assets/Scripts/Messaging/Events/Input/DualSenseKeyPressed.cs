using Game.Controls.DualSense;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Events.Input
{
	public class DualSenseKeyPressed: Message
	{
		public readonly DualSenseInput Shortcut;

		public DualSenseKeyPressed(object sender, DualSenseInput shortcut): base(sender)
		{
			Shortcut = shortcut;
		}
	}
}
