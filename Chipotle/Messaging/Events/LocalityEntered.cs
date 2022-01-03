using System;

using Game.Entities;
using Game.Terrain;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Tells a <see cref="Game.Terrain.Locality">object that an <see
    /// cref="Game.Entities.Entity">has entered.</see>/&gt;</see>/&gt;that an NPC
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    [Serializable]
    public class LocalityEntered : GameMessage
    {
        /// <summary>
        /// The NPC that entered the locality
        /// </summary>
        public readonly Entity Entity;

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
        public LocalityEntered(object sender, Entity entity, Locality locality) : base(sender)
        {
             Entity = entity;
            this.Locality = locality;
        }
    }
}