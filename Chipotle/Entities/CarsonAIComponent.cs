using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using ProtoBuf;

using System.Linq;

namespace Game.Entities
{
    /// <summary>
    /// Controls the behavior of the Carson NPC.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
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

            SetPosition message = new SetPosition(this, new Rectangle("1230, 1017"), true); // Sitting on a bench at a table
            InnerMessage(message);
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case CharacterLeftLocality ll: OnLocalityLeft(ll); break;
                case CharacterCameToLocality le: OnLocalityEntered(le); break;
                default: base.HandleMessage(message); break;
            }
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
        private void OnLocalityEntered(CharacterCameToLocality message)
        {
            if (message.Character != World.Player || message.Locality != Owner.Locality)
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
        private void OnLocalityLeft(CharacterLeftLocality message)
        {
            if (message.Locality != Owner.Locality || message.Character != World.Player)
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
            Item car = World.GetObject("detektivovo auto");
            if (_saidGoodbyeToChipotle && !road.IsItHere(car)) // Chipotle left the area
            {
                _messagingEnabled = false;
                InnerMessage(new Destroy(this));
            }
        }
    }
}