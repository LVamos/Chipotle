using Game.Entities;
using Game.Entities.Items;

using System;
using System.Collections.Generic;

namespace Game.Messaging.Commands.UI
{
	/// <summary>
	///  Instructs a window to run a voice menu to select an item or character to which another object will be applied.
	/// </summary>
	[Serializable]
	public class SelectObjectToApply : Message
	{
		/// <summary>
		/// An item to be applied to another item or character.
		/// </summary>
		public readonly Item ItemToApply;
		/// <summary>
		/// The characters or items to be used
		/// </summary>
		public readonly List<Entity> Objects;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sender of the message</param>
		/// <param name="itemToApply">An item to be applied to another item or character</param>
		/// <param name="objects">The items or characters to be applied</param>
		public SelectObjectToApply(object sender, Item itemToApply, List<Entity> objects) : base(sender)
		{
			ItemToApply = itemToApply;
			Objects = objects;
		}
	}
}