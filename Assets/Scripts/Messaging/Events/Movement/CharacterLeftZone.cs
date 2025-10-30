using Game.Entities.Characters;
using Game.Terrain;

using System;

namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Tells a <see cref="Zone">object that an <see
	/// cref="Character">has left.</see>/&gt;</see>/&gt;that an NPC
	/// </summary>
	/// <remarks>Sent from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.</remarks>
	[Serializable]
	public class CharacterLeftZone : Message
	{
		public readonly Zone LeftZone;
		public readonly Zone NewZone;

		/// <summary>
		/// The character that left a zone
		/// </summary>
		public readonly Character Character;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">The object that sent this message</param>
		/// <param name="character">The NPC that left the zone</param>
		/// <param name="leftZone">The zone left by the NPC</param>
		/// <param name="newZone">The zone the character came to</param>
		public CharacterLeftZone(object sender, Character character, Zone leftZone, Zone newZone) : base(sender)
		{
			Character = character;
			LeftZone = leftZone;
			NewZone = newZone;
		}
	}
}