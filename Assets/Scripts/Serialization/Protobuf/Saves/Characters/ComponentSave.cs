using Game.Serialization.Protobuf.Snapshots.Entities;

using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ComponentSave : EntitySave
	{
		public string Owner { get; set; }
	}
}
