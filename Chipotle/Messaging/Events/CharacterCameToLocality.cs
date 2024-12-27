using Game.Entities;
using Game.Terrain;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Tells a <see cref="Game.Terrain.Locality">object that an <see
    /// cref="Game.Entities.Character">has entered.</see>/&gt;</see>/&gt;that an NPC
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.</remarks>
    public class CharacterCameToLocality : GameMessage
    {
        /// <summary>
        /// The NPC that entered the locality
        /// </summary>
        public readonly Character Character;

        /// <summary>
        /// The concerning locality
        /// </summary>
        public readonly Locality Locality;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="entity">The NPC that entered the locality</param>
        /// <param name="locality">The concerning locality</param>
        public CharacterCameToLocality(object sender, Character entity, Locality locality) : base(sender)
        {
            Character = entity;
            this.Locality = locality;
        }
    }
}