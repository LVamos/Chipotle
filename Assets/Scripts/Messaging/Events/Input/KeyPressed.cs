using System;
using Game.UI;

namespace Game.Messaging.Events.Input
{
	/// <summary>
	/// Indicates that the player fired a keyboard command.
	/// </summary>
	[Serializable]
	public class KeyPressed : Message
	{
		/// <summary>
		/// The key combination that was pressed
		/// </summary>
		public readonly KeyShortcut Shortcut;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="shortcut">The key combination that was pressed</param>
		public KeyPressed(object sender, KeyShortcut shortcut) : base(sender)
			=> Shortcut = shortcut;
	}
}