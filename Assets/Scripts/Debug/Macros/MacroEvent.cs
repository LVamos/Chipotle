using Game.Controls.Keyboard;

namespace Game.Debug
{
	/// <summary>
	/// One recorded keyboard event with relative delay
	/// </summary>
	public struct MacroEvent
	{
		public int DelayMilliseconds;
		public MacroEventType EventType;
		public KeyboardInput Shortcut;
		public char Character;
	}
}