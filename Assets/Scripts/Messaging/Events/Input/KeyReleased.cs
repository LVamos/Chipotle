using Game.Controls.Keyboard;

using System;

namespace Game.Messaging.Events.Input
{
	[Serializable]
	public class KeyReleased : Message
	{
		public readonly KeyboardInput Shortcut;

		public KeyReleased(object sender, KeyboardInput shortcut) : base(sender) 
		{
			Shortcut = shortcut;
		}
	}
}