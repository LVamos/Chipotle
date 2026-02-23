using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class SoundSnapshot : CharacterSnapshot
	{
		public bool AnnounceWalls { get; set; }
		public float WalkVolume { get; set; }
	}
}
