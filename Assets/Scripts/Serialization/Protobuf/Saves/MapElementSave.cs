using Game.Serialization.Protobuf.Snapshots.Entities;
using Game.Serialization.Protobuf.Snapshots.Spatial;
using Game.Serialization.Protobuf.Snapshots.Spatial.Passages;
using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Serialization.Protobuf.Snapshots
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(EntitySave))]
	[ProtoInclude(101, typeof(ZoneSave))]
	[ProtoInclude(102, typeof(PassageSave))]
	public class MapElementSave
	{
		public Rectangle? Area { get; set; }
		public Dictionary<string, string> Sounds { get; set; }
		public Name Name { get; set; }
		public bool Usable { get; set; }
		public List<string> UsableWith { get; set; }
	}
}
