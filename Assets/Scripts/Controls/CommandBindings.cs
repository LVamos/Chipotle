using Game.Controls.DualSense;
using Game.Controls.Keyboard;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Controls
{
	public class CommandBindings
	{
		public KeyboardInput? Keyboard;
		public DualSenseInput? DualSense;

		public CommandBindings(KeyboardInput? keyboard, DualSenseInput? dualSense)
		{
			Keyboard = keyboard;
			DualSense = dualSense;
		}
	}
}
