using System;
using System.Collections.Generic;
using Game.Entities.Items;

namespace Game.Messaging.Commands.UI
{
	/// <summary>
	/// Instructs a window to run a vocie menu to select an item to be picked up
	/// </summary>
	[Serializable]
	public class SelectItemToPick : Message
	{
		/// <summary>
		/// The items to be picked up
		/// </summary>
		public List<Item> Items { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sender of the message</param>
		/// <param name="objects">The items to be picked up</param>
		public SelectItemToPick(object sender, List<Item> objects) : base(sender)
		{
			Items = objects;
		}
	}
}