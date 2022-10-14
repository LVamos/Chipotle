namespace Game.Messaging.Commands
{
    public class SayExits : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public SayExits(object sender) : base(sender) { }
    }
}