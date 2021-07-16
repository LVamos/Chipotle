using Game.Terrain;

namespace Game.Messaging.Events
{
	class PassageShown : GameMessage
	{
		public readonly Passage Passage;

		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="passage">New passage</param>
		public PassageShown (MessagingObject sender, Passage passage) : base(sender) => Passage = passage;

	}
}
