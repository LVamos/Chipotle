using Game.Messaging.Events.Movement;
using Game.Terrain;

using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters.Tuttle
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class TuttleAISnapshot : AISnapshot
	{
		public ChipotlesCarMoved CarMovement { get; set; }
		public int CollisionInterval { get; set; }
		public bool GoToPoolWhenPositionSet { get; set; }
		public bool PlayerWasByPool { get; set; }
		public Zone _ridingTo { get; set; }
	}
}
