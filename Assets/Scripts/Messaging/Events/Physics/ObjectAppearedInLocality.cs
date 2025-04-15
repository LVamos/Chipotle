using Game.Entities.Items;
using Game.Terrain;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Informs that a game object appeared in a zone.
	/// </summary>
	public class ObjectAppearedInZone : Message
	{
		/// <summary>
		/// The object that appeared in the zone.
		/// </summary>
		public readonly Item Object;

		/// <summary>
		/// The zone the object apeared in
		/// </summary>
		public readonly Zone Zone;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="object">The object that appeared in the zone</param>
		/// <param name="zone">The concerning zone</param>
		public ObjectAppearedInZone(object sender, Item @object, Zone zone) : base(sender)
		{
			Object = @object;
			Zone = zone;
		}
	}
}