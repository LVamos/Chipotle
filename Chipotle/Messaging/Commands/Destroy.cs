namespace Game.Messaging.Commands
{
    public class Destroy : GameMessage
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public Destroy(object sender) : base(sender)
        {
        }
    }
}
