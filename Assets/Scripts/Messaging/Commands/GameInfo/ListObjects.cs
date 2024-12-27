namespace Game.Messaging.Commands
{
	public class ListObjects : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public ListObjects(object sender) : base(sender) { }
	}
}