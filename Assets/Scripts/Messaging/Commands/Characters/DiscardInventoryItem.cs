using Game.Entities.Items;

namespace Game.Messaging.Commands.Characters
{
	public class DiscardInventoryItem : Message
	{
		public Item Item { get; }

		public DiscardInventoryItem(MessagingObject sender, Item item) : base(sender)
		{
			Item = item;
		}
	}
}
