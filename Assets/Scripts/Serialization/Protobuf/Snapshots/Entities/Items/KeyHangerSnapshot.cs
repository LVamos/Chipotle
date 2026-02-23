using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Entities.Items.Items
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class KeyHangerSnapshot : ItemSnapshot
	{
		public bool KeysHanging { get; set; }
	}
}
