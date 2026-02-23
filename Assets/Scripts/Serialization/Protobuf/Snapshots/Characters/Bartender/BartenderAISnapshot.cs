using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters.Bartender
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class BartenderAISnapshot : AISnapshot
	{
		public bool SayGoodbyeToChipotle { get; set; }
		public bool VelcomeChipotle { get; set; }
		public bool WasChipotleHere { get; set; }
	}
}
