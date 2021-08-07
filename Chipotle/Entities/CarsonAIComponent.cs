using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Game.Entities
{
    public class CarsonAIComponent : AIComponent
    {


        private bool _yelledAtChipotle;
        private bool _saidGoodbyeToChipotle;

        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
                    [typeof(LocalityEntered)] = (m) => OnLocalityEntered((LocalityEntered)m),
                    [typeof(LocalityLeft)] = (m) => OnLocalityLeft((LocalityLeft)m)

                }
                );
            SetPosition message = new SetPosition(this, new Plane("1229, 1017"), true); // Sitting on a bench at a table
            Owner.ReceiveMessage(message);
        }

        private void OnLocalityLeft(LocalityLeft message)
        {
            if (message.Sender != World.Player)
                return;

            bool benchUsed = World.GetObjectsByType("lavice u carsona").Any(o => o.Used);
            if (benchUsed && !_saidGoodbyeToChipotle)
            {
                _saidGoodbyeToChipotle = true;
                World.PlayCutscene(Owner, "cs33");
            }
        }



        private void OnLocalityEntered(LocalityEntered message)
        {
            if (message.Sender != World.Player)
                return;

            // This happens just once.
            if (!_yelledAtChipotle)
            {
                _yelledAtChipotle = true;
                World.PlayCutscene(Owner, "cs34");
            }
        }

        public override void Update()
        {
            base.Update();

            if (!_messagingEnabled)
                return;

            WatchChipotlesCar();
        }

        private void WatchChipotlesCar()
        {
            Locality road = World.GetLocality("asfaltka c1");
            DumpObject car = World.GetObject("detektivovo auto");
            if (_saidGoodbyeToChipotle && !road.IsItHere(car)) // Chipotle left the area
            {
                _messagingEnabled = false;
                Owner.ReceiveMessage(new Destroy(this));
            }
        }


    }
}
