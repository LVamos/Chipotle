using Game.Terrain;

using System;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Indicates that a door was opened or closed.
	/// </summary>
	[Serializable]
	public class DoorManipulated : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public DoorManipulated(Door sender) : base(sender)
		{
			Sender = sender;
		}

		/// <summary>
		/// The door that was open or closed.
		/// </summary>
		public readonly new Door Sender;
	}
}