﻿namespace Game.Messaging.Events
{
	class CutsceneBegan: GameMessage
	{
		public readonly string SoundName;


		/// <summary>
		/// Constructs instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="soundName">Identifier of the audio cut scene file</param>
		public CutsceneBegan(object sender, string soundName) : base(sender) => SoundName=soundName;

	}
}