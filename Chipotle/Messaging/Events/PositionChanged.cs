using Game.Terrain;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC changed its position.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    public class PositionChanged : GameMessage
    {
        /// <summary>
        /// New position of the NPC
        /// </summary>
        public readonly Plane Area;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="area">New position of the NPC</param>
        public PositionChanged(object sender, Plane area) : base(sender)
            => Area = new Plane(area);
    }
}
