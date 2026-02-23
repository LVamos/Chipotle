using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(107, typeof(AISnapshot))]
	[ProtoInclude(108, typeof(PhysicsSnapshot))]
	[ProtoInclude(109, typeof(SoundSnapshot))]
	public class CharacterComponentSnapshot
	{
		public string Owner { get; set; }
	}
}
