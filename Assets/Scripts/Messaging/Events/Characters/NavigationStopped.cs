using System;

namespace Game.Messaging.Events.Characters
{
	/// <summary>
	/// Informs that a map element navigation has been stopped.
	/// </summary>
	[Serializable]
	public class NavigationStopped : Message
	{
		public bool TargetReached;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public NavigationStopped(object sender, bool targetReached) : base(sender) 
		{
			TargetReached = targetReached;
		}
	}
}