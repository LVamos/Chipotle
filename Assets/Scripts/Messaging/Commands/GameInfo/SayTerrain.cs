using System;

namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Tells an NPC to report the terrain it's currently standing on.
	/// </summary>
	/// <remarks>
	/// Applies to the <see cref="Entities.Characters.Character"/> class. Can be sent only from inside the
	/// NPC from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.
	/// </remarks>
	[Serializable]
	public class SayTerrain : Message
	{
		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public SayTerrain(object sender) : base(sender) { }
	}
}