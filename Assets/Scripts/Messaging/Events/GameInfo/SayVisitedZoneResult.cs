using System;

namespace Game.Messaging.Events.GameInfo
{
	/// <summary>
	/// Makes the Chipotle NPC announce if it have already visited the zone it's currently
	/// located in.
	/// </summary>
	[Serializable]
	public class SayVisitedZoneResult : Message
	{
		/// <summary>
		/// Indicates if the region in which the concerning NPC is currently located has been previously visited.
		/// </summary>
		public readonly bool Visited;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="visited">Indicates if the region in which the concerning NPC is currently located has been previously visited.</param>
		public SayVisitedZoneResult(object sender, bool visited) : base(sender) => Visited = visited;
	}
}