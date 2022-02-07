using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Represents the grill object in the zahrada c1 locality.
    /// </summary>
    [Serializable]
    public class CarsonsGrill : DumpObject
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name for the object</param>
        /// <param name="area">The coordinates of the area that the object occupies</param>
        public CarsonsGrill(Name name, Plane area, bool decorative) : base(name, area, "gril u Carsona", decorative, null, null, "snd17")
        { }

        /// <summary>
        /// Initializes the object and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
                    [typeof(LocalityLeft)] = (message) => OnLocalityLeft((LocalityLeft)message)
                }
                );
        }

        /// <summary>
        /// Handles the LocalityLeft message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        private void OnLocalityLeft(LocalityLeft message)
        {
            if (message.Sender == World.GetEntity("carson"))
                StopLoop();
        }
    }
}