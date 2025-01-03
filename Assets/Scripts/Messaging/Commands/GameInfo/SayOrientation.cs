﻿namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Reports current orientation setting of an NPC.
	/// </summary>
	public class SayOrientation : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public SayOrientation(object sender) : base(sender) { }
	}
}