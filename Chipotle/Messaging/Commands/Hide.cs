namespace Game.Messaging.Commands
{
    /// <summary>
    /// Hides an NPC.
    /// </summary>
    /// <remarks>Applies to the <see cref="Game.Entities.Entity"/> class. Can be sent only from inside the NPC from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
    public class Hide : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public Hide(object sender) : base(sender) { }
    }
}
