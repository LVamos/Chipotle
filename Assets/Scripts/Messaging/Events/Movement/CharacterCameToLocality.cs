using Game.Entities.Characters;
using Game.Terrain;

namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Tells a <see cref="Terrain.Zone">object that an <see
	/// cref="Entities.Characters.Character">has entered.</see>/&gt;</see>/&gt;that an NPC
	/// </summary>
	/// <remarks>Sent from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.</remarks>
	public class CharacterCameToZone : Message
	{
		/// <summary>
		/// The NPC that entered the zone
		/// </summary>
		public readonly Character Character;

		/// <summary>
		/// The zone the character came to
		/// </summary>
		public readonly Zone CurrentZone;

		/// <summary>
		/// The zone the character came from
		/// </summary>
		public readonly Zone PreviousZone;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="character">The NPC that entered the zone</param>
		/// <param name="currentZone">The concerning zone</param>
		public CharacterCameToZone(object sender, Character character, Zone currentZone, Zone previousZone) : base(sender)
		{
			Character = character;
			CurrentZone = currentZone;
			PreviousZone = previousZone;
		}
	}
}