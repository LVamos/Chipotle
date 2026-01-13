using Game;
using Game.Entities.Items;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Entities.Items
{
	public class WalshesKeys : Item
	{
		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, bool passable = false, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
		{
			base.Initialize(name, area, "walshovy klíče", decorative: false, pickable: true, usable, passable: true, collisionSound, actionSound, loopSound, cutscene, usableOnce: true, audibleOverWalls: true, volume, stopWhenPlayerMoves, quickActionsAllowed, pickingSound, placingSound);
			UsableWith = new() { "dbp w1" };
		}
	}
}
