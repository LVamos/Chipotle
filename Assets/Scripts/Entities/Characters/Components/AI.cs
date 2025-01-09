using Game.Entities.Characters.Bartender;
using Game.Entities.Characters.Carson;
using Game.Entities.Characters.Christine;
using Game.Entities.Characters.Mariotti;
using Game.Entities.Characters.Sweeney;
using Game.Entities.Characters.Tuttle;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Events.Movement;

using ProtoBuf;

using UnityEngine;

using Message = Game.Messaging.Message;
using Rectangle = Game.Terrain.Rectangle;

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
		/// Jumps to a specific position.
		/// </summary>
		/// <param name="position">The position to jump to considered a center of the character.</param>
		/// <param name="silently">Whether to jump silently or not. Default is true.</param>
		protected void JumpTo(Vector2 position, bool silently = true)
		{
			Vector3 dimensions = gameObject.transform.localScale;
			_area = Rectangle.FromCenter(position, dimensions.z, dimensions.x);
			InnerMessage(new SetPosition(this, new(_area), silently));
		}

		/// <summary>
		/// Processes the PositionChanged message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnPositionChanged(PositionChanged message)
		{
			_area = message.TargetPosition;
		}
	}
}