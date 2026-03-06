using Game.Entities.Characters;
using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;

using UnityEngine;

namespace Game.Serialization.Protobuf.Snapshots.Entities.Items.Items
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(105, typeof(ChipotlesCarSave))]
	[ProtoInclude(106, typeof(KeyHangerSave))]
	public class ItemSave : EntitySave
	{
		public string CollisionSound { get; set; }
		public string ActionSound { get; set; }
		public string LoopSound { get; set; }
		public string Cutscene { get; set; }
		public string PickingSound { get; set; }
		public string PlacingSound { get; set; }
		public bool AudibleOverWalls { get; set; }
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
