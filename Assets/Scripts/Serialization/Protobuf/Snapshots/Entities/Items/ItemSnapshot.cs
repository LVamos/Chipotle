using Game.Entities.Characters;
using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;

using UnityEngine;

namespace Game.Serialization.Protobuf.Snapshots.Entities.Items.Items
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(105, typeof(ChipotlesCarSnapshot))]
	[ProtoInclude(106, typeof(KeyHangerSnapshot))]
	public class ItemSnapshot : EntitySnapshot
	{
		public bool AudibleOverWalls { get; set; }
		public string Cutscene { get; set; }
		public ObstacleType LastOccludingObstacle { get; set; }
		public Vector3? LoopPositionBackup { get; set; }
		public bool Pickable { get; set; }
		public bool QuickActionsAllowed { get; set; }
		public bool StopWhenPlayerMoves { get; set; }
		public bool UsableOnce { get; set; }
		public HashSet<string> Zones { get; set; }
		public bool Decorative { get; set; }
		public Character HeldBy { get; set; }
		public bool Passable { get; set; }
		public bool Used { get; set; }
		public bool UsedOnce { get; set; }
	}
}
