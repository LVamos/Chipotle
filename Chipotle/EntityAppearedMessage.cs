using Game.Entities;

namespace Game
{
	public class EntityAppearedMessage: Message
	{
		public readonly Entity Entity;

		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="newEntity">the entity which just appeared</param>
		public EntityAppearedMessage(MessagingObject sender, Entity entity) : base(sender) => Entity= entity;

	}
}