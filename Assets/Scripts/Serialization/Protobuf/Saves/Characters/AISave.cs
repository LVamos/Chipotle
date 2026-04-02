using Game.Entities;
using Game.Serialization.Protobuf.Saves;
using Game.Serialization.Protobuf.Snapshots.Characters.Bartender;
using Game.Serialization.Protobuf.Snapshots.Characters.Carson;
using Game.Serialization.Protobuf.Snapshots.Characters.Tuttle;

using ProtoBuf;

namespace Game.Serialization.Protobuf.Snapshots.Characters
{
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(110, typeof(BartenderAISave))]
	[ProtoInclude(111, typeof(CarsonAISave))]
	[ProtoInclude(112, typeof(TuttleAISave))]
	public class AISave : ComponentSave
	{
		public RectangleSave Area { get; set; }
		public bool Hidden { get; set; }
		public float MaxObjectDistance { get; set; }
		public float MinObjectDistance { get; set; }
		public CharacterState State { get; set; }
	}
}
