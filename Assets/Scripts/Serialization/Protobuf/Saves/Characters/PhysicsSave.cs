using Game.Entities;
using Game.Serialization.Protobuf.Snapshots.Characters.Chipotle;
using Game.Terrain;

using ProtoBuf;

using System.Collections.Generic;

using UnityEngine;

namespace Game.Serialization.Protobuf.Snapshots.Characters
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(120, typeof(ChipotlePhysicsSave))]
	public class PhysicsSave : ComponentSave
	{
		public CharacterState State { get; set; }
		public Rectangle? Area { get; set; }
		public Vector2 Goal { get; set; }
		public HashSet<string> Inventory { get; set; }
		public float MaxObjectDistance { get; set; }
		public float MinObjectDistance { get; set; }
		public int NavigableObjectsRadius { get; set; }
		public List<Vector2> NearbyWalls { get; set; }
		public float ObjectManipulationHelpRadius { get; set; }
		public Orientation2D Orientation { get; set; }
		public Queue<Vector2> Path { get; set; }
		public bool RestartApproaching { get; set; }
		public int Speed { get; set; }
		public float StepLength { get; set; }
		public int TargetPlayerDistance { get; set; }
		public float WallDistanceThreshold { get; set; }
		public float Height { get; set; } = .4f;
		public Rectangle? StartPosition { get; set; }
		public float Width { get; set; } = .4f; //todo předělat
	}
}
