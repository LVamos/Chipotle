using Game.Entities;

using System;
using System.Collections.Generic;

namespace Game.Messaging.Commands.UI
{
	[Serializable]
	public class SelectObjectToExplore : Message
	{
		/// <summary>
		/// The objects to be explored
		/// </summary>
		public List<Entity> Objects { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sender of the message</param>
		/// <param name="objects">The objects to be explored</param>
		public SelectObjectToExplore(object sender, List<Entity> objects) : base(sender)
		{
			Objects = objects;
		}
	}
}