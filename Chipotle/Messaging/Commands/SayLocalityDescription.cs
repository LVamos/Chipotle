namespace Game.Messaging.Commands
{
    /// <summary>
    /// Instructs a sound component of a character to read description of the current locality.
    /// </summary>
    public class SayLocalityDescription : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Souruce of the message</param>
        public SayLocalityDescription(object sender) : base(sender)
        {
        }
    }
}
