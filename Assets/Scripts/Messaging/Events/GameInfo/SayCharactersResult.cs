using Game.Models;

using NUnit.Framework;

using System;
using System.Collections.Generic;

namespace Game.Messaging.Events.GameInfo
{
	/// <summary>
	/// Result of the SayCharacters command
	/// </summary>
	[Serializable]
	public class SayCharactersResult : Message
	{
		/// <summary>
		///  The characters in range.
		/// </summary>
		public List<NavigableCharacterInfo> Characters { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sender of the message</param>
		/// <param name="characters">The characters in range</param>
		public SayCharactersResult(object sender, List<NavigableCharacterInfo> characters) : base(sender)
			=> Characters = characters;
	}
}