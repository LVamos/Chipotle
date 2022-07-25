using Game.Entities;
using Game.Terrain;

namespace Game.Messaging.Events
{
	/// <summary>
	/// Informs a game object or an entity that it was hit by a door when another entity tryied to close the door.
	/// </summary>
	public class PinchedInDoor: GameMessage
	{
		public new Door Sender;
		public Entity Entity;

		public PinchedInDoor(Door sender, Entity entity):base(sender)
		{
			Sender = sender;
			Entity = entity;
		}
	}
}