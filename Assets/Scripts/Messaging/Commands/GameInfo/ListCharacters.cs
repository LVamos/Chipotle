using System;

namespace Game.Messaging.Commands
{
	/// <summary>
	/// Instructs the game to list all characters in range.
	/// </summary>
	[Serializable]
	public class ListCharacters : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sender of the message</param>
		public ListCharacters(object sender) : base(sender)
		{
		}
	}
}