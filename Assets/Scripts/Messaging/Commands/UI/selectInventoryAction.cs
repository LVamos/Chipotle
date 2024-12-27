using System;
using System.Collections.Generic;
using Game.Entities.Items;

namespace Game.Messaging.Commands.UI
{
	/// <summary>
	/// Instructs the game window to run a voice menu to select an item and coresponding action from inventory
	/// </summary>
	[Serializable]
	public class selectInventoryAction : Message
	{
		/// <summary>
		/// The items to be picked up
		/// </summary>
		public readonly List<Item> Items;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sender of the message</param>
		/// <param name="items">The items to be picked up</param>
		public selectInventoryAction(object sender, List<Item> items) : base(sender)
			=> Items = items;
	}
}