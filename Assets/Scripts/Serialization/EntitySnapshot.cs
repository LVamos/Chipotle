using ProtoBuf;

namespace Game.Serialization
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
