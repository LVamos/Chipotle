using System;
using Game.Entities.Characters.Components;

namespace Game.Messaging.Commands.UI
{
	public class RunInventoryMenu : Message
	{
		/// <summary>
		/// Source of the message.
		/// </summary>
		public new readonly CharacterComponent Sender;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <exception cref="ArgumentNullException"></exception>
		public RunInventoryMenu(CharacterComponent sender) : base(sender) => Sender = sender ?? throw new ArgumentNullException(nameof(sender));
	}
}