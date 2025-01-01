using Game.Entities.Characters;
using Game.Terrain;

using System;

namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Tells a <see cref="Locality">object that an <see
	/// cref="Character">has left.</see>/&gt;</see>/&gt;that an NPC
	/// </summary>
	/// <remarks>Sent from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.</remarks>
	[Serializable]
	public class CharacterLeftLocality : Message
	{
		public readonly Locality LeftLocality;
		public readonly Locality NewLocality;

		/// <summary>
		/// The character that left a locality
		/// </summary>
		public readonly Character Character;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">The object that sent this message</param>
		/// <param name="character">The NPC that left the locality</param>
		/// <param name="leftLocality">The locality left by the NPC</param>
		/// <param name="newLocality">The locality the character came to</param>
		public CharacterLeftLocality(object sender, Character character, Locality leftLocality, Locality newLocality) : base(sender)
		{
			Character = character;
			LeftLocality = leftLocality;
			NewLocality = newLocality;
		}
	}
}