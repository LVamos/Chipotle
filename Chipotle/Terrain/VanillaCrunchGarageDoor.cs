using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Events;

using Luky;

namespace Game.Terrain
{
    /// <summary>
    /// Represents the garage door in the garage of the Vanilla crunch company (garáž v1) locality.
    /// </summary>
    [Serializable]
    public class VanillaCrunchGarageDoor : Door
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the area occupied by the door</param>
        /// <param name="localities">Localities connected by the door</param>
        public VanillaCrunchGarageDoor(Name name, Plane area, IEnumerable<Locality> localities) : base(name, PassageState.Closed, area, localities, false) { }

        /// <summary>
        /// Initializes the door and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>()
                {
                    [typeof(LocalityEntered)] = (message) => OnLocalityEntered((LocalityEntered)message)
                });
        }

        /// <summary>
        /// Processes the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityEntered(LocalityEntered message)
        {
if(message.Locality == World.GetLocality("garáž v1") && message.Entity == World.Player)
            State = PassageState.Closed;
        }
    }
}