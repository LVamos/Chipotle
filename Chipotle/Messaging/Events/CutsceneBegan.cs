namespace Game.Messaging.Events
{
    public class CutsceneBegan : GameMessage
    {
        public readonly  string CutsceneName;
        public readonly int SoundID;

        /// <summary>
        /// Constructs instance of the message.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="soundName">Identifier of the audio cut scene file</param>
        public CutsceneBegan(object sender, string cutsceneName, int soundID) : base(sender)
        {
            CutsceneName = cutsceneName;
            SoundID = soundID;
        }

    }
}
