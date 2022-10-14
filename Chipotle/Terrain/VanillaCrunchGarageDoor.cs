using Game.Messaging;
using Game.Messaging.Events;

using Luky;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Terrain
{
    /// <summary>
    /// Represents the garage door in the garage of the Vanilla crunch company (garáž v1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class VanillaCrunchGarageDoor : Door
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the area occupied by the door</param>
        /// <param name="localities">Localities connected by the door</param>
        public VanillaCrunchGarageDoor(Name name, Rectangle area, IEnumerable<string> localities) : base(name, PassageState.Closed, area, localities, false) { }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case CharacterCameToLocality le: OnLocalityEntered(le); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Processes the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityEntered(CharacterCameToLocality message)
        {
            if (message.Locality == World.GetLocality("garáž v1") && message.Character == World.Player)
                State = PassageState.Closed;
        }
    }
}