using Game.Entities;

namespace Game.Messaging.Events.GameActions
{
	public class ExplorationObjectSelected : Message
	{
		public Entity Object { get; }

		public ExplorationObjectSelected(object sender, Entity @object) : base(sender)
		{
			Object = @object;
		}
	}
}
