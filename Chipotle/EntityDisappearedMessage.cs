using Game.Entities;

namespace Game
{
	public class EntityDisappearedMessage: EntityAppearedMessage
	{
		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="newEntity">The entity which just disappeared</param>
		public EntityDisappearedMessage(MessagingObject sender, Entity entity) : base(sender, entity) { }

	}
}
