using Game.Serialization.Protobuf.Snapshots.Characters;
using Game.Serialization.Protobuf.Snapshots.Entities.Items.Items;

using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Entities
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(103, typeof(ItemSave))]
	[ProtoInclude(104, typeof(CharacterSave))]
	public class EntitySave : MapElementSave
	{
		public int DescriptionID { get; set; }
		public string Type { get; set; }
	}
}
