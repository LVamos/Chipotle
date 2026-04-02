using ProtoBuf;

namespace Game.Serialization.Protobuf.Saves
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class Vector2Save
	{
		public float X { get; set; }
		public float Y { get; set; }
	}
}
