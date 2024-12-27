using Game.Entities;

using System;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	///  A command to explore an item or character.
	/// </summary>
	/// <remarks>
	/// If the object is empty, it means that physics has yet to find out if there are any k objects in front of the character.
	/// </remarks>	
	[Serializable]
	public class ExploreObject : Message
	{
		/// <summary>
		/// The object to be explored
		/// </summary>
		public Entity Object { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sender of the message</param>
		/// <param name="obj">The object to be explored</param>
		public ExploreObject(object sender, Entity obj = null) : base(sender)
		{
			Object = obj;
		}
	}
}