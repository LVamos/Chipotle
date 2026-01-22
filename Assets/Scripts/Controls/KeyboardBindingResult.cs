using Game.Controls.Keyboard;

namespace Game.Controls
{
	public class KeyboardBindingResult
	{
		public readonly KeyboardInput? Shortcut;
		public readonly CommandId? CommandWithSameShortcut;
		public readonly CommandId Command;

		public KeyboardBindingResult(CommandId command, KeyboardInput? shortcut = null, CommandId? commandWithSameShortcut = null)
		{
			Command = command;
			Shortcut = shortcut;
			CommandWithSameShortcut = commandWithSameShortcut;
		}
	}
}
