using Game;
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
		public void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable)
			=> base.Initialize(name, area, type, decorative, pickable, usable);
	}
}
