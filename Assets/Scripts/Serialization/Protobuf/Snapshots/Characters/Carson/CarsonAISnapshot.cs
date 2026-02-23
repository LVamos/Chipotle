using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters.Carson
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class CarsonAISnapshot : AISnapshot
	{
		public bool SaidGoodbyeToChipotle { get; set; }
		public bool YelledAtChipotle { get; set; }
	}
}
