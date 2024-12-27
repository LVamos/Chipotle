using System;

namespace Game.Messaging.Events.Characters
{
	/// <summary>
	/// Informs that a map element navigation has been stopped.
	/// </summary>
	[Serializable]
	public class NavigationStopped : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public NavigationStopped(object sender) : base(sender) { }
	}
}