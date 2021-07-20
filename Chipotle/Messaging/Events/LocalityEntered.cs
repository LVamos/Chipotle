using Game.Entities;

namespace Game.Messaging.Events
{
	public class LocalityEntered : GameMessage
	{
		public readonly Entity Entity;

		public LocalityEntered(object sender, Entity entity):base(sender)
		{
			this.Entity= entity;
		}
	}
}
