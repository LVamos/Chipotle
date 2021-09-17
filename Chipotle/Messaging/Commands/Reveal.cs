using Game.Terrain;



namespace Game.Messaging.Commands
{
    /// <summary>
    /// Shows a hidden NPC.
    /// </summary>
    /// <remarks>Applies to the <see cref="Game.Entities.Entity"/> class. Can be sent only from inside the NPC from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    public class Reveal : GameMessage
    {
        /// <summary>
        /// The position at which the NPC is displayed.
        /// </summary>
        public readonly Plane Location;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="location">The position at which the NPC is displayed</param>
        public Reveal(object sender, Plane location) : base(sender)
            => Location = location;
    }
}
