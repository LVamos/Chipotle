namespace Game.Messaging.Events
{
	class CutsceneEnded: CutsceneBegan
	{

		/// <summary>
		/// Constructs instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="soundName">Identifier of the audio cut scene file</param>
		public CutsceneEnded(object sender, string soundName) : base(sender, soundName) { }

	}
}
