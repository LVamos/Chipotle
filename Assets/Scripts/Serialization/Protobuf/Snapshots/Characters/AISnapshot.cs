using Game.Entities;
using Game.Serialization.Protobuf.Snapshots.Characters.Bartender;
using Game.Serialization.Protobuf.Snapshots.Characters.Carson;
using Game.Serialization.Protobuf.Snapshots.Characters.Tuttle;
using Game.Terrain;

using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(110, typeof(BartenderAISnapshot))]
	[ProtoInclude(111, typeof(CarsonAISnapshot))]
	[ProtoInclude(112, typeof(TuttleAISnapshot))]
	public class AISnapshot : CharacterComponentSnapshot
	{
		public Rectangle Area { get; set; }
		public bool Hidden { get; set; }
		public float MaxObjectDistance { get; set; }
		public float MinObjectDistance { get; set; }
		public CharacterState State { get; set; }
	}
}
