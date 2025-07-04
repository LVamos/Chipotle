﻿using Game.Entities.Characters.Components;
using Game.Messaging.Events.Sound;

using ProtoBuf;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Christine
{
	/// <summary>
	/// Controls behavior of the Christine Pierce NPC
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ChristineAI : AI
	{
		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void Activate()
		{
			base.Activate();
			JumpNear(World.GetItem("stůl p1").Area.Value); // At a table in her dining room
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case CutsceneEnded ce: OnCutsceneEnded(ce); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Processes the CutsceneEnded message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnCutsceneEnded(CutsceneEnded message)
		{
			base.OnCutsceneEnded(message);

			switch (message.CutsceneName)
			{
				case "cs21": JumpToBedroom(); break;
			}
		}

		/// <summary>
		/// Moves the NPC to the Christine's bed room (ložnice p1) zone.
		/// </summary>
		private void JumpToBedroom()
		{
			JumpNear(World.GetItem("zrcadlo p1").Area.Value);
		}
	}
}