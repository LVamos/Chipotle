namespace Game.Controls
{
	public class BindingResult
	{
		public readonly bool Success;
		public readonly CommandId? CommandWithSameShortcut;
		public readonly CommandId Command;

		public BindingResult(CommandId command, bool success = true, CommandId? commandWithSameShortcut = null)
		{
			Command = command;
			Success = success;
			CommandWithSameShortcut = commandWithSameShortcut;
		}
	}
}
