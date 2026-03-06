using Game.Terrain;

using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Spatial.Passages
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class DoorSave : PassageSave
	{
		public string ClosingSound { get; set; }
		public string LockedSound { get; set; }
		public string OpeningSound { get; set; }
		public DoorType Type { get; set; }
	}
}
