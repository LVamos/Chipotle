using Game.Controls.DualSense;

namespace Assets.Scripts.Controls
{
	public class DualSenseBindingResult
	{
		public readonly DualSenseInput? Shortcut;
		public readonly bool ShortcutAlreadyUsed;

		public DualSenseBindingResult(DualSenseInput? shortcut, bool shortcutAlreadyUsed)
		{
			Shortcut = shortcut;
			ShortcutAlreadyUsed = shortcutAlreadyUsed;
		}
	}
}
