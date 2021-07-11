using Game.Terrain;

namespace Game
{
	class PassageAppearedMessage: Message
	{
		public readonly Passage Passage;

		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="passage">New passage</param>
		public PassageAppearedMessage(MessagingObject sender, Passage passage) : base(sender) => Passage = passage;

	}
}
