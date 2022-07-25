using Game.Entities;

namespace Game.Messaging.Commands
{
	/// <summary>
	/// instructs a component of an entity to react to pinching in a door.
	/// </summary>
	public class ReactToPinchingInDoor : GameMessage
	{
		/// <summary>
		/// A component of an entity that sent the message.
		/// </summary>
		public new EntityComponent Sender;
		
		/// <summary>
		/// An entity that tried to close the door.
		/// </summary>
		public Entity Entity;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">A component of an entity that sent the message.</param>
		/// <param name="entity">An entity that tried to close the door.</param>
		public ReactToPinchingInDoor(EntityComponent sender, Entity entity) : base(sender)
		{
			Sender = sender;
			Entity = entity;
		}
	}
}