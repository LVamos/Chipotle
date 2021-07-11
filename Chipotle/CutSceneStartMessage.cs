namespace Game
{
	class CutSceneStartMessage: Message
	{
		public readonly string SoundName;


		/// <summary>
		/// Constructs instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="soundName">Identifier of the audio cut scene file</param>
		public CutSceneStartMessage(object sender, string soundName) : base(sender) => SoundName=soundName;

	}
}
