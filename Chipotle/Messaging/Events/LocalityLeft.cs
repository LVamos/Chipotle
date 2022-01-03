using System;

using Game.Entities;
using Game.Terrain;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Tells a <see cref="Game.Terrain.Locality">object that an <see
    /// cref="Game.Entities.Entity">has left.</see>/&gt;</see>/&gt;that an NPC
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    [Serializable]
    public class LocalityLeft : LocalityEntered
    {
/// <summary>
/// Constructor
/// </summary>
/// <param name="sender">The object that sent this message</param>
/// <param name="entity">The NPC that left the locality</param>
/// <param name="locality">The locality left by the NPC</param>
        public LocalityLeft(object sender, Entity entity, Locality locality): base(sender, entity, locality)
        { }
           }
}