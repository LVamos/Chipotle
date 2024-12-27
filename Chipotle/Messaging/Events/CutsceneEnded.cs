using System;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an audio cutscene has just finished.
    /// </summary>
    [Serializable]
    public class CutsceneEnded : CutsceneBegan
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="cutsceneName">Name of the played cutscene</param>
        /// <param name="soundID">Handle of the sound object for the played cutscene</param>
        public CutsceneEnded(object sender, string cutsceneName, int soundID) : base(sender, cutsceneName, soundID) { }
    }
}