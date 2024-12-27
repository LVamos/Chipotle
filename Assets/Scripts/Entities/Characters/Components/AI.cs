using Game.Entities.Characters.Bartender;
using Game.Entities.Characters.Carson;
using Game.Entities.Characters.Christine;
using Game.Entities.Characters.Mariotti;
using Game.Entities.Characters.Sweeney;
using Game.Entities.Characters.Tuttle;
using Game.Messaging.Events.Movement;
using Game.Terrain;

using ProtoBuf;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Components
{
	/// <summary>
	/// Controls the behavior of an NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(BartenderAI))]
	[ProtoInclude(101, typeof(CarsonAI))]
	[ProtoInclude(102, typeof(ChristineAI))]
	[ProtoInclude(103, typeof(MariottiAI))]
	[ProtoInclude(104, typeof(SweeneyAI))]
	[ProtoInclude(105, typeof(TuttleAI))]
	public class AI : CharacterComponent
	{
		/// <summary>
		/// Area occupied by the NPC
		/// </summary>
		protected Rectangle _area;

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case PositionChanged pc: OnPositionChanged(pc); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Processes the PositionChanged message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnPositionChanged(PositionChanged message)
			=> _area = message.TargetPosition;
	}
}