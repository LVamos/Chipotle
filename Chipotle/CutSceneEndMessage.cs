namespace Game
{
	class CutSceneEndMessage: CutSceneStartMessage
	{

		/// <summary>
		/// Constructs instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="soundName">Identifier of the audio cut scene file</param>
		public CutSceneEndMessage(object sender, string soundName) : base(sender, soundName) { }

	}
}
