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
    public abstract class Entity: GameObject
    {
        public Orientation2D Orientation { get => _physics.Orientation; }

        public new Plane Area { get => _physics.Area; }


        public override void ReceiveMessage(GameMessage message)
        {
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
        public static Chipotle CreatePlayer()
            => new Chipotle(new ChipotleInputComponent(), new ChipotlePhysicsComponent(), new ChipotleSoundComponent());





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
        public Entity(Name name, string type, PhysicsComponent physics, SoundComponent sound, AIComponent ai, InputComponent input):base(name, type, null)
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
