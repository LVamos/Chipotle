namespace Game.Messaging.Commands
{
    public class SayLocality : GameMessage
    {
        /// <summary>
        /// Instructs the NPC to report the name of the locality in which it is currently located.
        /// </summary>
        /// <remarks>Applies to the <see cref="Game.Entities.Entity"/> class. Can be sent only from inside the NPC from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.</remarks>
        public SayLocality(object sender) : base(sender)
        {
        }
    }
}