using ProtoBuf;

namespace Game.Serialization
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class KeyHangerSnapshot : ItemSnapshot
	{
		public bool KeysHanging { get; set; }
	}
}
