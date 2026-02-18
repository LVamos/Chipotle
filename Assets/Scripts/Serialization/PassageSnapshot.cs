using Game.Terrain;

using ProtoBuf;

namespace Game.Serialization
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(106, typeof(DoorSnapshot))]
	public class PassageSnapshot : MapElementSnapshot
	{
		public Zone PlayersZone { get; set; }
		public string[] Zones { get; set; }
		public PassageState State { get; set; } = PassageState.Open;
		public string TypeDescription { get; set; }
	}
}
