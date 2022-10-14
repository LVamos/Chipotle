namespace Game.Messaging.Events
{
    public class SayObjectsResult : GameMessage
    {
        /// <summary>
        /// Information about the nearest objects
        /// </summary>
        public readonly string[] Objects;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Sorce of the message</param>
        /// <param name="objectInfo">Information about the nearest objects</param>
        public SayObjectsResult(object sender, string[] objects) : base(sender)
        => Objects = objects;
    }
}
