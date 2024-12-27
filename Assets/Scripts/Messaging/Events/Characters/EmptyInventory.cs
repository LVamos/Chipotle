namespace Game.Messaging.Events.Characters
{
	/// <summary>
	/// Tells an entity component that its inventory is empty.
	/// </summary>
	public class EmptyInventory : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public EmptyInventory(object sender) : base(sender)
		{
		}
	}
}