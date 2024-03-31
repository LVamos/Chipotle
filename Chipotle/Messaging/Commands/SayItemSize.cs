namespace Game.Messaging.Commands
{
	/// <summary>
	/// Represents a game command message for saying the size of an item.
	/// </summary>
	public class SayItemSize : GameMessage
	{
		/// <summary>
		/// Initializes a new instance of the SayItemSize class.
		/// </summary>
		/// <param name="sender">The sender object.</param>
		public SayItemSize(object sender) : base(sender)

		{
		}
	}
}
