using Game.Entities;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Tells a <see cref="Game.Terrain.Locality"> object that an <see cref="Game.Entities.Entity"> has left.</see>/> </see>/>that an NPC 
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    public class LocalityLeft : GameMessage
    {

        /// <summary>
        /// The NPC that leftthe locality
        /// </summary>
        public readonly Entity Entity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="entity">The NPC that left the locality</param>
        public LocalityLeft(object sender, Entity entity) : base(sender) => Entity = entity;
    }
}
