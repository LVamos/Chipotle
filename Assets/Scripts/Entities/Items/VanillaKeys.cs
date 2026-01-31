using Game.Messaging;
using Game.Messaging.Commands.Physics;
using Game.Terrain;

using System.Collections.Generic;

namespace Game.Entities.Items
{
	public class VanillaKeys : Item
	{
		public override void Initialize(
			Name name,
			Rectangle area,
			string type,
			bool decorative,
			bool pickable,
			bool usable,
			bool passable = false,
			string collisionSound = null,
			string actionSound = null,
			string loopSound = null,
			string cutscene = null,
			bool usableOnce = false,
			bool audibleOverWalls = true,
			float volume = 1,
			bool stopWhenPlayerMoves = false,
			bool quickActionsAllowed = false,
			string pickingSound = null,
			string placingSound = null,
			List<string> usableWith = null)
		{
			base.Initialize(
				name,
				area,
				type,
				decorative,
				true,
				false,
				true,
				collisionSound,
				actionSound,
				loopSound,
				cutscene,
				false,
				audibleOverWalls,
				volume,
				stopWhenPlayerMoves,
				quickActionsAllowed,
				pickingSound,
				placingSound,
				new() { "věšák v1" }
				);
		}

		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case UseObjects m: OnUseObjects(m); break;
				default: base.HandleMessage(message); break;
			}
		}

		private void OnUseObjects(UseObjects message)
		{
			MapElement target = message.Target;
			if (target != null && target.Name.Indexed == "věšák v1")
				_cutscene = "HangKeys";
			else _cutscene = null;

			base.OnUseObjects(message);
		}
	}
}
