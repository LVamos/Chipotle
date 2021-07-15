using Game.Entities;

namespace Game
{
	public class LocalityEntered : Message
	{
		public readonly object Sender;
		public readonly Entity Entity;

		public LocalityEntered(object sender, Entity entity):base(sender)
		{
			this.Entity= entity;
		}
	}
}
