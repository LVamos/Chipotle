
using Game.Entities;

namespace Game
{
	class ObjectDisappeared: Message
	{
		public readonly GameObject Object;

		public ObjectDisappeared(object sender, GameObject o) : base(sender) => Object=o;

	}
}
