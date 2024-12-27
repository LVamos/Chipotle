using System;

namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Instructs the NPC to report names of the closest objects in its surroundings.
	/// </summary>
	/// <remarks>
	/// Applies to the <see cref="Entities.Characters.Character"/> class. Can be sent only from inside the
	/// NPC from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.
	/// </remarks>
	[Serializable]
	public class SayObjects : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public SayObjects(object sender) : base(sender) { }
	}
}