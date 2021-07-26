namespace Game.Messaging.Events
{
    public class CutsceneBegan : GameMessage
    {


        /// <summary>
        /// Constructs instance of the message.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="soundName">Identifier of the audio cut scene file</param>
        public CutsceneBegan(object sender) : base(sender)
        { }

    }
}
