using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Serialization
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ChipotlesCarSnapshot : ItemSnapshot
	{
		public HashSet<string> AllowedDestinations { get; set; }
		public HashSet<Zone> VisitedZones { get; set; }
	}
}
