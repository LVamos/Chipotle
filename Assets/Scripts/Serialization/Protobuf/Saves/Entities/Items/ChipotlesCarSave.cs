using Game.Serialization.Protobuf.Snapshots.Entities.Items.Items;
using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Serialization.Protobuf.Snapshots.Entities.Items
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ChipotlesCarSave : ItemSave
	{
		public HashSet<string> AllowedDestinations { get; set; }
		public HashSet<Zone> VisitedZones { get; set; }
	}
}
