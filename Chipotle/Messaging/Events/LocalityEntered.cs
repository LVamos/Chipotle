using Game.Entities;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Tells a <see cref="Game.Terrain.Locality"> object that an <see cref="Game.Entities.Entity"> has entered.</see>/> </see>/>that an NPC 
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    public class LocalityEntered : GameMessage
    {
        /// <summary>
        /// The NPC that entered the locality
        /// </summary>
        public readonly Entity Entity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="entity">The NPC that entered the locality</param>
        public LocalityEntered(object sender, Entity entity) : base(sender) => Entity = entity;
    }
}
