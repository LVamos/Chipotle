using Game.Entities.Characters.Components;
using Game.Messaging.Commands.Movement;
using ProtoBuf;

namespace Game.Entities.Characters.Mariotti
{
	/// <summary>
	/// Controls behavior of the Paolo Mariotti NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class MariottiAI : AI
	{
		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();
			InnerMessage(new SetPosition(this, new("2018, 1131"), true));
		}
	}
}