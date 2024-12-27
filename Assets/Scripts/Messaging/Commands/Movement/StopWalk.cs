using System;

namespace Game.Messaging.Commands.Movement
{
	[Serializable]
	public class StopWalk : Message
	{
		public StopWalk(object sender) : base(sender) { }
	}
}