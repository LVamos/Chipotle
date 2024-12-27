namespace Game.Messaging.Commands.GameInfo
{
	public class SayExits : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public SayExits(object sender) : base(sender) { }
	}
}