using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;

using UnityEngine;

namespace Game.Serialization.Protobuf.Snapshots.Spatial
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ZoneSave : MapElementSave
	{
		public float Ceiling { get; set; }
		public HashSet<string> Exits { get; set; }
		public HashSet<string> Characters { get; set; }
		public HashSet<string> Items { get; set; }
		public List<string> Neighbours { get; set; }
		public HashSet<Vector2> Nonwalkables { get; set; }
		public TerrainType DefaultTerrain { get; set; }
		public string Description { get; set; }
		public string To { get; set; }
		public ZoneType Type { get; set; }
	}
}
