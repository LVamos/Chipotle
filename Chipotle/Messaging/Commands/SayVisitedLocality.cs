namespace Game.Messaging.Commands
{
    /// <summary>
    /// Makes the Chipotle NPC announce if it have already visited the locality it's currently
    /// located in.
    /// </summary>
    public class SayVisitedLocality : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public SayVisitedLocality(object sender) : base(sender) { }
    }
}