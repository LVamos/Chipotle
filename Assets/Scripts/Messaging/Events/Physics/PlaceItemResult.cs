using Game.Entities.Items;

using System;

namespace Game.Messaging.Events.Physics
{
	public class PlaceItemResult : Message
	{
		/// <summary>
		/// Result of placing an object to the ground
		/// </summary>
		public readonly bool Success;

		/// <summary>
		/// Soruce fo the message.
		/// </summary>
		public new readonly object Sender;

		public readonly Item Item;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="result">Result of the operation</param>
		/// <exception cref="ArgumentNullException"></exception>
		public PlaceItemResult(object sender, Item item, bool success = false) : base(sender)
		{
			Sender = sender ?? throw new ArgumentNullException(nameof(sender));
			Item = item;
			Success = success;
		}
	}
}