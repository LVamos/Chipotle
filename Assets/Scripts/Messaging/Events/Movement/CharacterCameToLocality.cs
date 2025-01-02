using Game.Entities.Characters;
using Game.Terrain;

namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Tells a <see cref="Terrain.Locality">object that an <see
	/// cref="Entities.Characters.Character">has entered.</see>/&gt;</see>/&gt;that an NPC
	/// </summary>
	/// <remarks>Sent from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.</remarks>
	public class CharacterCameToLocality : Message
	{
		/// <summary>
		/// The NPC that entered the locality
		/// </summary>
		public readonly Character Character;

		/// <summary>
		/// The locality the character came to
		/// </summary>
		public readonly Locality CurrentLocality;

		/// <summary>
		/// The locality the character came from
		/// </summary>
		public readonly Locality PreviousLocality;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="character">The NPC that entered the locality</param>
		/// <param name="currentLocality">The concerning locality</param>
		public CharacterCameToLocality(object sender, Character character, Locality currentLocality, Locality previousLocality) : base(sender)
		{
			Character = character;
			CurrentLocality = currentLocality;
			PreviousLocality = previousLocality;
		}
	}
}