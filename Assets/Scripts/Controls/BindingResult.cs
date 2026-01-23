namespace Game.Controls
{
	public class BindingResult
	{
		public readonly CommandId? CommandWithSameShortcut;
		public readonly CommandId Command;

		public BindingResult(CommandId command, CommandId? commandWithSameShortcut = null)
		{
			Command = command;
			CommandWithSameShortcut = commandWithSameShortcut;
		}
	}
}
