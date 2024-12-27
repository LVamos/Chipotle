using System;
using System.Collections.Generic;
using Game.Entities;

namespace Game.Messaging.Commands.UI
{
	/// <summary>
	///  Instructs a window to run a voice menu to select an object to use
	/// </summary>
	[Serializable]
	public class SelectObjectToUse : Message
	{
		/// <summary>
		/// The objects to be used
		/// </summary>
		public readonly List<Entity> Objects;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sender of the message</param>
		/// <param name="objects">The objects to be used</param>
		public SelectObjectToUse(object sender, List<Entity> objects) : base(sender)
			=> Objects = objects;
	}
}