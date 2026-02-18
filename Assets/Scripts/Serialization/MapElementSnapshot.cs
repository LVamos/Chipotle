using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Serialization
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(EntitySnapshot))]
	[ProtoInclude(101, typeof(ZoneSnapshot))]
	[ProtoInclude(102, typeof(PassageSnapshot))]
	public class MapElementSnapshot
	{
		public Rectangle? Area { get; set; }
		public Dictionary<string, string> Sounds { get; set; }
		public Name Name { get; set; }
		public bool Usable { get; set; }
		public List<string> UsableWith { get; set; }
	}
}
