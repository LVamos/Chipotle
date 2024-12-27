using Game.Messaging.Events.Movement;

using ProtoBuf;

using Message = Game.Messaging.Message;
using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	/// <summary>
	/// Represents the grill object in the zahrada c1 locality.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class CarsonsGrill : Item
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner and public name for the object</param>
		/// <param name="area">The coordinates of the area that the object occupies</param>
		public void Initialize(Name name, Rectangle area, string type, bool decorative, bool pickable, bool usable)
			=> base.Initialize(name, area, type, decorative, pickable, usable, null, null, "snd17", volume: 1);

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case CharacterLeftLocality ll: OnLocalityLeft(ll); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Handles the LocalityLeft message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnLocalityLeft(CharacterLeftLocality message)
		{
			if (message.Sender == World.GetCharacter("carson"))
				StopLoop();
		}
	}
}