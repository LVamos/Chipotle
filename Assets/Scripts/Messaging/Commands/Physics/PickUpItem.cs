using Game.Entities.Items;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Tells an entity to pick up an object off the ground.
	/// </summary>
	public class PickUpItem : Message
	{
		/// <summary>
		/// Source of the message (Entity or EntityComponent).
		/// </summary>
		public new readonly MessagingObject Sender;

		/// <summary>
		/// The object that should be picked.
		/// </summary>
		public readonly Item Item;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message (Entity or EntityComponent)</param>
		/// <param name="item">The object that should be picked</param>
		public PickUpItem(MessagingObject sender, Item item = null) : base(sender)
		{
			Sender = sender;
			Item = item;
		}
	}
}