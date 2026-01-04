namespace Game.Messaging.Commands
{
	public class ListItems : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public ListItems(object sender) : base(sender) { }
	}
}