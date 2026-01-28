using Game.Entities.Items;

namespace Game.Messaging.Events.GameActions
{
	public class ItemForPickingSelected : Message
	{
		public Item Item { get; }

		public ItemForPickingSelected(object sender, Item item) : base(sender)
		{
			Item = item;
		}
	}
}
