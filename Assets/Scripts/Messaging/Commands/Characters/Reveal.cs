using System;
using Rectangle = Game.Terrain.Rectangle;

namespace Game.Messaging.Commands.Characters
{
	/// <summary>
	/// Shows a hidden NPC.
	/// </summary>
	/// <remarks>
	/// Applies to the <see cref="Entities.Characters.Character"/> class. Can be sent only from inside the
	/// NPC from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.
	/// </remarks>
	[Serializable]
	public class Reveal : Message
	{
		/// <summary>
		/// The position at which the NPC is displayed.
		/// </summary>
		public readonly Rectangle Location;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="location">The position at which the NPC is displayed</param>
		public Reveal(object sender, Rectangle location) : base(sender)
			=> Location = location;
	}
}