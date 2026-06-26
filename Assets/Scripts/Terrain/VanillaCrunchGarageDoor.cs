using Game.Messaging.Events.Movement;



using System.Collections.Generic;

using Message = Game.Messaging.Message;

namespace Game.Terrain
{
    /// <summary>
    /// Represents the garage door in the garage of the Vanilla crunch company (garáž v1) zone.
    /// </summary>

    public class VanillaCrunchGarageDoor : Door
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the area occupied by the door</param>
        /// <param name="zones">Zones connected by the door</param>
        public override void Initialize(Name name, PassageState state, Rectangle area, IEnumerable<string> zones, DoorType type = DoorType.Door, bool usable = false)
            => base.Initialize(name, PassageState.Closed, area, zones);

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(Message message)
        {
            switch (message)
            {
                case CharacterCameToZone le: OnZoneEntered(le); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Processes the ZoneEntered message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnZoneEntered(CharacterCameToZone message)
        {
            Zone garage = World.GetZone("garáž v1");
            if (message.CurrentZone == garage && message.Character == World.Player)
                State = PassageState.Closed;
        }
    }
}