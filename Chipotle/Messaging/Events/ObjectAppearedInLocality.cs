using Game.Entities;
using Game.Terrain;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Informs that a game object appeared in a locality.
    /// </summary>
    public class ObjectAppearedInLocality : GameMessage
    {
        /// <summary>
        /// The object that appeared in the locality.
        /// </summary>
        public readonly Item Object;

        /// <summary>
        /// The locality the object apeared in
        /// </summary>
        public readonly Locality Locality;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="object">The object that appeared in the locality</param>
        /// <param name="locality">The concerning locality</param>
        public ObjectAppearedInLocality(object sender, Item @object, Locality locality) : base(sender)
        {
            Object = @object;
            Locality = locality;
        }
    }
}
