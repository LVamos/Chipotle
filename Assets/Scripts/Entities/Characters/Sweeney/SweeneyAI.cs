using Game.Entities.Characters.Components;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Events.Sound;
using ProtoBuf;
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
			InnerMessage(new SetPosition(this, new("1402, 960"), true));
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
				case "cs23": JumpToSweeneysRoom(); break;
			}
		}

		/// <summary>
		/// Relocates the Detective Chipotle and Tuttle NPCs from Easterby street (ulice s1)
		/// locality to the sweeney's hall (hala s1) locality.
		/// </summary>
		private void JumpToSweeneysRoom()
			=> InnerMessage(new SetPosition(this, new("1407, 978"), true));
	}
}