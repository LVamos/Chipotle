using System;

namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Tells a map element to start announcing its position with a sound loop.
	/// </summary>
	[Serializable]
	public class StartNavigation : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public StartNavigation(object sender) : base(sender)
		{
		}
	}
}