using Game.Entities.Items;
using Game.Terrain;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Informs that a object disappeared from a locality.
	/// </summary>
	public class ObjectDisappearedFromLocality : ObjectAppearedInLocality
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="object">The object that disappeared from the locality</param>
		/// <param name="locality">The locality the object dissapeared from</param>
		public ObjectDisappearedFromLocality(object sender, Item @object, Locality locality) : base(sender, @object, locality) { }
	}
}