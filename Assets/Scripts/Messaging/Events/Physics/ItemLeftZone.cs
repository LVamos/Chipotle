using Game.Entities.Items;
using Game.Terrain;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Informs that a object disappeared from a zone.
	/// </summary>
	public class ItemLeftZone : ItemAppearedInZone
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="item">The object that disappeared from the zone</param>
		/// <param name="zone">The zone the object dissapeared from</param>
		public ItemLeftZone(object sender, Item item, Zone zone) : base(sender, item, zone) { }
	}
}