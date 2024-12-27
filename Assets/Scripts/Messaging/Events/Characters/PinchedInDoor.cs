using Game.Entities.Characters;
using Game.Terrain;

namespace Game.Messaging.Events.Characters
{
	/// <summary>
	/// Informs a game object or an entity that it was hit by a door when another entity tryied to close the door.
	/// </summary>
	public class PinchedInDoor : Message
	{
		public new Door Sender;
		public Character Entity;

		public PinchedInDoor(Door sender, Character entity) : base(sender)
		{
			Sender = sender;
			Entity = entity;
		}
	}
}