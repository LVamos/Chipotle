using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

namespace Game.Entities
{
    /// <summary>
    /// Controls the behavior of the Carson NPC.
    /// </summary>
    [Serializable]
    public class CarsonAIComponent : AIComponent
    {
        /// <summary>
        /// Indicates if the Carson NPC said goodbye to the Detective Chipotle NPC when Chipotle
        /// left the zahrada c1 locality.
        /// </summary>
        private bool _saidGoodbyeToChipotle;

        /// <summary>
        /// Indicates if the Carson NPC scolded the Detective Chipotle NPC the first time Chipotle
        /// came to the zahrada c1 locality.
        /// </summary>
        private bool _yelledAtChipotle;

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
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

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (!_messagingEnabled)
                return;

            WatchChipotlesCar();
        }

        /// <summary>
        /// Processes the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
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

        /// <summary>
        /// Processes the LocalityLeft message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
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

        /// <summary>
        /// checks if the Detective's car object went away from the asfaltka c1 locality after
        /// saying goodbye.
        /// </summary>
        /// <remarks>The method is called from the Update method.</remarks>
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