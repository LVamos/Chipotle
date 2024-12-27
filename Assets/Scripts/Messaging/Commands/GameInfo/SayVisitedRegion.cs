using System;

namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Makes the Chipotle NPC announce if it have already visited the locality it's currently
	/// located in.
	/// </summary>
	[Serializable]
	public class SayVisitedRegion : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public SayVisitedRegion(object sender) : base(sender) { }
	}
}