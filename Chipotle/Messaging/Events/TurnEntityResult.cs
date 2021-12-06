using System;

using Game.Terrain;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC has completed a rotation.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    [Serializable]
    public class TurnEntityResult : GameMessage
    {
        /// <summary>
        /// New orientation of the NPC
        /// </summary>
        private Orientation2D NewOrientation;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="orientation">New orientation of the NPC</param>
        public TurnEntityResult(object sender, Orientation2D orientation) : base(sender) => NewOrientation = orientation;
    }
}