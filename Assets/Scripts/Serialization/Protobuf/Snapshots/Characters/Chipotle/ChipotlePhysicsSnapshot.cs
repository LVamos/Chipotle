namespace Game.Serialization.Protobuf.Snapshots.Characters.Chipotle
{
	[ProtoBuf.ProtoContract(SkipConstructor = true, ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
	public class ChipotlePhysicsSnapshot : PhysicsSnapshot
	{
		public int CurrentRegion { get; set; }
		public bool InVisitedRegion { get; set; }
		public bool PhoneCountdown { get; set; }
		public int PhoneDeltaTime { get; set; }
		public int PhoneInterval { get; set; }
		public bool SittingAtPubTable { get; set; }
		public bool SittingOnChair { get; set; }
		public bool SteppedIntoPuddle { get; set; }
		public bool WalshesBenchUsed { get; set; }
	}
}
