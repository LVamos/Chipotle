using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using DavyKager;

using Luky;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Game.Terrain;

namespace Game.Entities
{
    public class Entity: GameObject
    {
        public Orientation2D Orientation { get => _physics.Orientation; }

        public new Plane Area { get => _physics.Area; }


        public override void ReceiveMessage(GameMessage message)
        {
            if (message.Sender == this)
                return;

            base.ReceiveMessage(message);

            if(_messagingEnabled)
            SendInnerMessage(message);
        }


        protected AIComponent _ai;
		protected InputComponent _input;
		protected PhysicsComponent _physics;
        protected SoundComponent _sound;


        /// <summary>
        /// Creates the Columbo entity
        /// </summary>
        /// <returns>Instance of Columbo</returns>
        public static Entity CreateChipotle()
            => new Entity(new Name("Chipotle", "detektiv Chipotle"), "Chipotle", null, new ChipotleInputComponent(), new ChipotlePhysicsComponent(), new ChipotleSoundComponent());

        public static Entity CreateBartender()
            => new Entity(new Name("Bartender", "pingl"), "Bartender", new BartenderAIComponent(), null, new BartenderPhysicsComponent(), new BartenderSoundComponent());

        public static Entity CreateCarson()
            => new Entity(new Name("Carson", "David Carson"), "Carson", new CarsonAIComponent(), null, new CarsonPhysicsComponent(), new CarsonSoundComponent());

        public static Entity CreateChristine()
            => new Entity(new Name("Christine", "Christine Piercová"), "Christine", new ChristineAIComponent(), null, new ChristinePhysicsComponent(), new ChristineSoundComponent());

        public static Entity CreateMariotti()
            => new Entity(new Name("Mariotti", "Paolo Mariotti"), "Mariotti", new MariottiAIComponent(), null, new MariottiPhysicsComponent(), new MariottiSoundComponent());

        public static Entity CreateSweeney()
            => new Entity(new Name("Sweeney", "Derreck Sweeney"), "Sweeney", new SweeneyAIComponent(), null, new SweeneyPhysicsComponent(), new SweeneySoundComponent());

        public static Entity CreateTuttle()
            => new Entity(new Name("Tuttle", "parťák"), "Tuttle", new TuttleAIComponent(), null, new TuttlePhysicsComponent(), new TuttleSoundComponent());


        /// <summary>
        /// Sends message to al components.
        /// </summary>
        /// <param name="message">Message to redistribute</param>
        protected virtual void SendInnerMessage(GameMessage message)
        {
            foreach(var c in _components)
            {
                if (c != message.Sender)
                    c.ReceiveMessage(message);
            }
        }

        protected List<EntityComponent> _components;




        /// <summary>
        /// Updates state of all components and lets them react on other game objects.
        /// </summary>
        public override void Update()
        {

            if (_messages.Count > 0)
                HandleMessage(DequeueMessage());
            _components.ForEach(c => c.Update());
        }


        public override void Start()
        {
            base.Start();
            _components.ForEach(c =>    c.Start());
        }



        /// <summary>
        /// Constructor
        /// </summary>
        public Entity(Name name, string type, AIComponent ai, InputComponent input, PhysicsComponent physics, SoundComponent sound):base(name, type, null)
        {
            _physics = physics;
            _sound = sound;
            _ai = ai;
            _input = input;
            _components = (new EntityComponent[] { ai, input, sound, physics }).Where(c => c != null).ToList<EntityComponent>();



            _components.ForEach(c => c.Owner = this);
        }
    }
}
