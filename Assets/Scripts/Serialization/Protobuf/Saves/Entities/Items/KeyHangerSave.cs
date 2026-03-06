using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Entities.Items.Items
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class KeyHangerSave : ItemSave
	{
		public bool KeysHanging { get; set; }
	}
}
