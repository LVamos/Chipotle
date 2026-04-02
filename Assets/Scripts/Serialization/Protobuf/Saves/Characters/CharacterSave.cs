using Game.Serialization.Protobuf.Saves;
using Game.Serialization.Protobuf.Snapshots.Entities;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Serialization.Protobuf.Snapshots.Characters
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class CharacterSave : EntitySave
	{
		public HashSet<string> Inventory { get; set; }
		public HashSet<string> VisitedZones { get; set; }
		public string Zone { get; set; }
		public Vector2Save Orientation { get; set; }
		public List<ComponentSave> Components { get; set; }
	}
}
