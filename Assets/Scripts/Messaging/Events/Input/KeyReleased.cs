using System;

using Game.Controls.Keyboard;
using Game.UI;

namespace Game.Messaging.Events.Input
{
	[Serializable]
	public class KeyReleased : KeyPressed
	{
		public KeyReleased(object sender, KeyboardInput shortcut) : base(sender, shortcut) { }
	}
}