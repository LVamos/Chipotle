// No changes needed as the file already includes `using Assets.Scripts.Entities.Items`.
using Game.Messaging.Events.Movement;

using ProtoBuf;

using Message = Game.Messaging.Message;
using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the grill object in the zahrada c1 zone.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class CarsonsGrill : Item
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner and public name for the object</param>
		/// <param name="area">The coordinates of the area that the object occupies</param>
		public override void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null)
					=> base.Initialize(name, area, type, decorative, pickable, usable, null, null, "snd17", volume: 1);

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case CharacterLeftZone ll: OnZoneLeft(ll); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Handles the ZoneLeft message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnZoneLeft(CharacterLeftZone message)
		{
			if (message.Sender == World.GetCharacter("carson"))
				StopLoop();
		}
	}
}