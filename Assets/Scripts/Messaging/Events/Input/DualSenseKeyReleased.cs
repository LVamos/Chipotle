using Game.Controls.DualSense;
using Game.Messaging;
using Game.Messaging.Events.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Messaging.Events.Input
{
	public class DualSenseKeyReleased: Message
	{
		public readonly DualSenseInput Shortcut;

		public DualSenseKeyReleased(object sender, DualSenseInput shortcut) : base(sender) 
		{
			Shortcut = shortcut;
		}
	}
}
