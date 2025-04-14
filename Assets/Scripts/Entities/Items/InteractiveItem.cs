using Game;
// No changes needed as the file already includes `using Assets.Scripts.Entities.Items`.
using Game.Entities.Items;
using Game.Terrain;

namespace Assets.Scripts.Entities.Items
{
	/// <summary>
	/// A base class for all interactive items.
	/// </summary>
	public class InteractiveItem : Item
	{
		/// <summary>
		/// Initializes the item
		/// </summary>
		/// <param name="name">Inner and public name for the object</param>
		/// <param name="area">The coordinates of the area that the object occupies</param>
		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
			=> base.Initialize(name, area, type, decorative, pickable, usable);
	}
}
