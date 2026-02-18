using Game.Terrain;

using ProtoBuf;

namespace Game.Serialization
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class DoorSnapshot : PassageSnapshot
	{
		public string ClosingSound { get; set; }
		public string LockedSound { get; set; }
		public string OpeningSound { get; set; }
		public DoorType Type { get; set; }
	}
}
