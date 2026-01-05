using Game.Models;

using System.Collections.Generic;

namespace Game.Messaging.Events.GameInfo
{
	public class SayItemsResult : Message
	{
		/// <summary>
		/// Information about the nearest objects
		/// </summary>
		public readonly List<NavigableItemInfo> Items;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sorce of the message</param>
		/// <param name="items">Information about the nearest objects</param>
		public SayItemsResult(object sender, List<NavigableItemInfo> items) : base(sender)
			=> Items = items;
	}
}