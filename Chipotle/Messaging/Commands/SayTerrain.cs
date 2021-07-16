namespace Game.Messaging.Commands
{
	class SayTerrain : GameMessage
	{
		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public SayTerrain (object sender):base(sender) { }
	}
}
