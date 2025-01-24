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
		public new readonly Item Sender;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="result">Result of the operation</param>
		/// <exception cref="ArgumentNullException"></exception>
		public PlaceItemResult(Item sender, bool success = false) : base(sender)
		{
			Sender = sender ?? throw new ArgumentNullException(nameof(sender));
			Success = success;
		}
	}
}