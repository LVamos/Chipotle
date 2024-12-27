using System;

namespace Game.Messaging.Commands
{
	/// <summary>
	/// Tells a map element to stop ongoing sound navigation.
	/// </summary>
	[Serializable]
	public class StopNavigation : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public StopNavigation(object sender) : base(sender) { }
	}
}