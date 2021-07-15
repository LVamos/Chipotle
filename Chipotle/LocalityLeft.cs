using Game.Entities;

namespace Game
{
	public class LocalityLeft: Message
	{
		public readonly object Sender;
		public readonly Entity Entity;

		public LocalityLeft(object sender, Entity entity):base(sender)
		{
			this.Entity= entity;
		}
	}
}
