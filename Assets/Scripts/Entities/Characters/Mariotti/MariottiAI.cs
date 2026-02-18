using Game.Entities.Characters.Components;



using UnityEngine;


namespace Game.Entities.Characters.Mariotti
{
	/// <summary>
	/// Controls behavior of the Paolo Mariotti NPC.
	/// </summary>
	
	public class MariottiAI : AI
	{
		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();
			JumpTo(_pointNearFreezer);
		}

		private Vector2 _pointNearFreezer = new(2013.3f, 1129.1f);
	}
}