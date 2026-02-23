using Game.Serialization.Protobuf.Snapshots.Entities;
using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Serialization.Protobuf.Snapshots.Characters
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class CharacterSnapshot : EntitySnapshot
	{
		public HashSet<string> Inventory { get; set; }
		public HashSet<string> VisitedZones { get; set; }
		public string Zone { get; set; }
		public Orientation2D Orientation { get; set; }
	}
}
