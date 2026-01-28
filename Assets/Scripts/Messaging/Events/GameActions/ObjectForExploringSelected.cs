using Game.Entities;

namespace Game.Messaging.Events.GameActions
{
	public class ObjectForExploringSelected : Message
	{
		public Entity Object { get; }

		public ObjectForExploringSelected(object sender, Entity @object) : base(sender)
		{
			Object = @object;
		}
	}
}
