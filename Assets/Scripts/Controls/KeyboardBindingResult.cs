using Game.Controls.Keyboard;

namespace Game.Controls
{
	public class KeyboardBindingResult
	{
		public readonly KeyboardInput? Shortcut;
		public readonly bool ShortcutAlreadyUsed;
		public readonly CommandId Command;

		public KeyboardBindingResult(CommandId command, KeyboardInput? shortcut = null, bool shortcutAlreadyUsed = false)
		{
			Command = command;
			Shortcut = shortcut;
			ShortcutAlreadyUsed = shortcutAlreadyUsed;
		}
	}
}
