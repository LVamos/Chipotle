using System;

namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Instructs the character to report names of the closest objects in its surroundings.
	/// </summary>
	/// <remarks>
	/// Applies to the <see cref="Entities.Characters.Character"/> class. Can be sent only from inside the
	/// NPC from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.
	/// </remarks>
	[Serializable]
	public class SayCharacters : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public SayCharacters(object sender) : base(sender) { }
	}
}