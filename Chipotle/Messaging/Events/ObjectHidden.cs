
using Game.Entities;

namespace Game.Messaging.Events
{
	class ObjectHidden : GameMessage
	{
		public readonly GameObject Object;

		public ObjectHidden (object sender, GameObject o) : base(sender) => Object=o;

	}
}
