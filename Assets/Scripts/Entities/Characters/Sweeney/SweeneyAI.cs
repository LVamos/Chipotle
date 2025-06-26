using Game.Entities.Characters.Components;
using Game.Messaging.Events.Sound;

using ProtoBuf;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Sweeney
{
	/// <summary>
	/// Controls behavior of the Derreck Sweeney NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class SweeneyAI : AI
	{
		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();
			JumpTo(_pointNearTable);
		}

		private Vector2 _pointNearTable = new(1402.3f, 955.7f);

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
				case "cs23": JumpToSweeneysRoom(); break;
			}
		}

		/// <summary>
		/// Relocates the Detective Chipotle and Tuttle NPCs from Easterby street (ulice s1)
		/// zone to the sweeney's hall (hala s1) zone.
		/// </summary>
		private void JumpToSweeneysRoom()
		{
			JumpTo(_pointNearBed);
		}
		private Vector2 _pointNearBed = new(1404.1f, 955.7f);
	}
}