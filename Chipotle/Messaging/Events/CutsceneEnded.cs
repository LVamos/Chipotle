namespace Game.Messaging.Events
{
	public class CutsceneEnded: GameMessage
	{

		/// <summary>
		/// Constructs instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="soundName">Identifier of the audio cut scene file</param>
		public CutsceneEnded(object sender) : base(sender) 
		{ }

	}
}
