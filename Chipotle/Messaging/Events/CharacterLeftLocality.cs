using Game.Entities;
using Game.Terrain;

using System;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Tells a <see cref="Game.Terrain.Locality">object that an <see
    /// cref="Game.Entities.Character">has left.</see>/&gt;</see>/&gt;that an NPC
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.</remarks>
    [Serializable]
    public class CharacterLeftLocality : CharacterCameToLocality
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">The object that sent this message</param>
        /// <param name="entity">The NPC that left the locality</param>
        /// <param name="locality">The locality left by the NPC</param>
        public CharacterLeftLocality(object sender, Character entity, Locality locality) : base(sender, entity, locality)
        { }
    }
}