namespace Game.Messaging.Commands
{
	public class ListExits : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public ListExits(object sender) : base(sender) { }
	}
}