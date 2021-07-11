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


        public override void ReceiveMessage(Message message)
        {
            base.ReceiveMessage(message);

            if(_messagingEnabled)
            SendInnerMessage(message);
        }


        protected PhysicsComponent _physics;
        protected SoundComponent _sound;

        //todo Entity

        /// <summary>
        /// Creates the Columbo entity
        /// </summary>
        /// <returns>Instance of Columbo</returns>
        public static Chipotle CreateColumbo()
            => new Chipotle(new ChipotleInputComponent(), new ChipotlePhysicsComponent(), new ChipotleSoundComponent());





        /// <summary>
        /// Sends message to al components.
        /// </summary>
        /// <param name="message">Message to redistribute</param>
        protected virtual void SendInnerMessage(Message message)
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
        public Entity(Name name, string type, PhysicsComponent physics, SoundComponent sound):base(name, type, null)
        {
            _physics = physics;
            _sound = sound;
            _components = new List<EntityComponent>
            {_physics, _sound };

            _components.ForEach(c => c.Owner = this);
        }
    }
}
