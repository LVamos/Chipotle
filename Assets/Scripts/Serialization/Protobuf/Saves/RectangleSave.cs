using ProtoBuf;

namespace Game.Serialization.Protobuf.Saves
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class RectangleSave
	{
		public Vector2Save UpperLeft { get; set; }
		public Vector2Save LowerRight { get; set; }
	}
}
