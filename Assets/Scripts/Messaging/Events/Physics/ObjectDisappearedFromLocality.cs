using Game.Entities.Items;
using Game.Terrain;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Informs that a object disappeared from a zone.
	/// </summary>
	public class ObjectDisappearedFromZone : ObjectAppearedInZone
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="object">The object that disappeared from the zone</param>
		/// <param name="zone">The zone the object dissapeared from</param>
		public ObjectDisappearedFromZone(object sender, Item @object, Zone zone) : base(sender, @object, zone) { }
	}
}