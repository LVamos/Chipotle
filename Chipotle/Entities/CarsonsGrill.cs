using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;


namespace Game.Entities
{
    public class CarsonsGrill : DumpObject
    {


        public CarsonsGrill(Name name, Plane area) : base(name, area, "gril u Carsona")
        { }

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

        private void OnLocalityEntered(LocalityEntered message)
        {
            if (message.Sender != World.Player)
                return;


            // Start loop if Carson is present
            Entity carson = World.GetEntity("carson");
            if (carson!=null && Locality.IsItHere(carson))
            {
                _sounds.loop = "snd17";
                _loopSoundId = World.Sound.Play(_sounds.loop, null, true, PositionType.Absolute, World.GetObject("gril c1").Area.Center, true);
            }
        }

    }
}
