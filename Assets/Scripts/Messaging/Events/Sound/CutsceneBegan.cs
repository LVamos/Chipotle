using System;

namespace Game.Messaging.Events.Sound
{
	/// <summary>
	/// Indicates that an audio cutscene has just started playing.
	/// </summary>
	[Serializable]
	public class CutsceneBegan : Message
	{
		/// <summary>
		/// Name of the played cutscene
		/// </summary>
		public readonly string CutsceneName;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="cutsceneName">Name of the played cutscene</param>
		/// <param name="soundID">Handle of the sound object for the played cutscene</param>
		public CutsceneBegan(object sender, string cutsceneName) : base(sender)
		{
			CutsceneName = cutsceneName;
		}
	}
}