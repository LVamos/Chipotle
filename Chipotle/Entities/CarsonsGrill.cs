using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using ProtoBuf;

namespace Game.Entities
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
        public CarsonsGrill(Name name, Rectangle area, bool decorative, bool pickable) : base(name, area, "gril u Carsona", decorative, pickable, null, null, "snd17", volume: 1)
        { }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
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