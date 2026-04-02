using ProtoBuf;

namespace Game.Serialization.Protobuf.Saves
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class NameSave
	{
		public string Indexed { get; set; }
		public string Friendly { get; set; }
	}
}
