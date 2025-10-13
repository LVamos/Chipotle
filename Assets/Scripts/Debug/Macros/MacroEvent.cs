using Game.Controls.Keyboard;
using Game.UI;

namespace Game.Debug
{
	/// <summary>
	/// One recorded key-down with relative delay
	/// </summary>
	public struct MacroEvent
	{
		public int DelayMilliseconds;
		public bool IsKeyDown;
		public KeyboardInput Shortcut;
	}
}