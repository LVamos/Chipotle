using Game.Serialization.Protobuf.Snapshots.Entities;

using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(300, typeof(AISave))]
	[ProtoInclude(301, typeof(PhysicsSave))]
	[ProtoInclude(302, typeof(SoundSave))]
	public class ComponentSave : EntitySave
	{
		public string Owner { get; set; }
	}
}
