namespace Game.Messaging.Commands
{
    /// <summary>
    /// Instructs a physics component of a character to read description of an object or character standing before the character.
    /// </summary>
    public class ResearchObject : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source fo the message</param>
        public ResearchObject(object sender) : base(sender)
        {
        }
    }
}
