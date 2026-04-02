using ProtoBuf;

namespace Game.Serialization.Protobuf.Saves
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class Vector3Save
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
	}
}
