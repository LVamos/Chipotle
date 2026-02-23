using Game.Serialization.Protobuf.Snapshots.Characters;
using Game.Serialization.Protobuf.Snapshots.Entities.Items.Items;

using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Entities
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(103, typeof(ItemSnapshot))]
	[ProtoInclude(104, typeof(CharacterSnapshot))]
	public class EntitySnapshot : MapElementSnapshot
	{
		public int DescriptionID { get; set; }
		public string Type { get; set; }
	}
}
