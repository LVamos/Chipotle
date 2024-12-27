using System;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Message that indicates that there are no walls nearby a NPC.
	/// </summary>
	[Serializable]
	public class NoWallsNearby : Message
	{
		public NoWallsNearby(object sender) : base(sender)
		{
		}
	}
}
