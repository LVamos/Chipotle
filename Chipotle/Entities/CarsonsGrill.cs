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
        public CarsonsGrill(Name name, Plane area, bool decorative) : base(name, area, "gril u Carsona", decorative)
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
                    [typeof(LocalityEntered)] = (m) => OnLocalityEntered((LocalityEntered)m)
                }
                );
        }

        /// <summary>
        /// Processes the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityEntered(LocalityEntered message)
        {
            if (message.Sender != World.Player || message.Locality != Locality)
                return;

            // Start loop if Carson is present
            if (Locality.IsItHere(World.GetEntity("carson")))
            {
                _sounds.loop = "snd17";
                _loopSoundId = World.Sound.Play(_sounds.loop, null, true, PositionType.Absolute, World.GetObject("gril c1").Area.Center, true);
            }
        }
    }
}