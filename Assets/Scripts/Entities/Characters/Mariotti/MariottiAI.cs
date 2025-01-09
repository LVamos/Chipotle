using Game.Entities.Characters.Components;

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
			JumpTo(new(2015.5f, 1129.3f)); // At the reffrigerator in the office
		}
	}
}