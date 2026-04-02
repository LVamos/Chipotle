using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters.Tuttle
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class TuttleAISave : AISave
	{
		public int CollisionInterval { get; set; }
		public bool GoToPoolWhenPositionSet { get; set; }
		public bool PlayerWasByPool { get; set; }
		public string CarTargetZone { get; set; }
	}
}
