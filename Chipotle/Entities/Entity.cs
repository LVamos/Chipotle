using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Represents an NPC.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class Entity : GameObject
    {
        /// <summary>
        /// List of all entity components
        /// </summary>
        protected EntityComponent[] _components;

        /// <summary>
        /// List of all localities visited by the NPC
        /// </summary>
        protected HashSet<string> _visitedLocalities;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public anme of the NPC</param>
        /// <param name="type">Type of the NPC</param>
        /// <param name="ai">Reference to an AI component</param>
        /// <param name="input">Reference to an input component</param>
        /// <param name="physics">Reference to an physics component</param>
        /// <param name="sound">Reference to an sound component</param>
        public Entity(Name name, string type, AIComponent ai, InputComponent input, PhysicsComponent physics, SoundComponent sound) : base(name, type, null)
        {
            _components = 
                new EntityComponent[] { ai, physics, input, sound }
                .Where(c => c != null)
                .ToArray<EntityComponent>();

            foreach (EntityComponent c in _components)
                c.AssignToEntity(name.Indexed);

            if (physics.StartPosition.HasValue)
            {
                Vector2 position = (Vector2)physics.StartPosition;
                _area = new Plane(position);
                _locality = _area.GetLocality();
            }
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case OrientationChanged och: OnOrientationChanged(och); break;
                case LocalityChanged lcd: OnLocalityChanged(lcd); break;
                case PositionChanged pcd: OnPositionChanged(pcd); break;
                default: base.HandleMessage(message); break;
            }

        }

        /// <summary>
        /// Handles the OrienttationChanged message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        private void OnOrientationChanged(OrientationChanged message)
            => Orientation = message.Target;

        /// <summary>
        /// Returns the current orientation of the NPC in the game world.
        /// </summary>
        public Orientation2D Orientation { get; private set; }

        /// <summary>
        /// List of all localities visited by the NPC
        /// </summary>
        public IEnumerable<Locality> VisitedLocalities
        {
            get
            {
                if (_visitedLocalities == null)
                    _visitedLocalities = new HashSet<string>();

                return _visitedLocalities.Select(l => World.GetLocality(l));
            }
        }

        /// <summary>
        /// Creates new instance of the Bartender NPC.
        /// </summary>
        /// <returns>New instance of the NPC</returns>
        public static Entity CreateBartender()
            => new Entity(new Name("Bartender", "pingl"), "Bartender", new BartenderAIComponent(), null, new PhysicsComponent(), new SoundComponent());

        /// <summary>
        /// Creates new instance of the Carson NPC.
        /// </summary>
        /// <returns>New instance of the NPC</returns>
        public static Entity CreateCarson()
            => new Entity(new Name("Carson", "David Carson"), "Carson", new CarsonAIComponent(), null, new PhysicsComponent(), new SoundComponent());

        /// <summary>
        /// Creates new instance of the Detective Chipotle NPC.
        /// </summary>
        /// <returns>New instance of the NPC</returns>
        public static Entity CreateChipotle()
            => new Entity(new Name("Chipotle", "detektiv Chipotle"), "Chipotle", null, new ChipotleInputComponent(), new ChipotlePhysicsComponent(), new ChipotleSoundComponent());

        /// <summary>
        /// Creates new instance of the Christine NPC.
        /// </summary>
        /// <returns>New instance of the NPC</returns>
        public static Entity CreateChristine()
            => new Entity(new Name("Christine", "Christine Piercová"), "Christine", new ChristineAIComponent(), null, new PhysicsComponent(), new SoundComponent());

        /// <summary>
        /// Creates new instance of the Mariotti NPC.
        /// </summary>
        /// <returns>New instance of the NPC</returns>
        public static Entity CreateMariotti()
            => new Entity(new Name("Mariotti", "Paolo Mariotti"), "Mariotti", new MariottiAIComponent(), null, new PhysicsComponent(), new SoundComponent());

        /// <summary>
        /// Creates new instance of the Sweeney NPC.
        /// </summary>
        /// <returns>New instance of the NPC</returns>
        public static Entity CreateSweeney()
            => new Entity(new Name("Sweeney", "Derreck Sweeney"), "Sweeney", new SweeneyAIComponent(), null, new PhysicsComponent(), new SoundComponent());

        /// <summary>
        /// Creates new instance of the Tuttle NPC.
        /// </summary>
        /// <returns>New instance of the NPC</returns>
        public static Entity CreateTuttle()
            => new Entity(new Name("Tuttle", "parťák"), "Tuttle", new TuttleAIComponent(), null, new TuttlePhysicsComponent(), new TuttleSoundComponent());

        /// <summary>
        /// Takes an incoming message and saves it into the message queue.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        public override void TakeMessage(GameMessage message)
        {
            base.TakeMessage(message);

            if (_messagingEnabled)
                SendInnerMessage(message);
        }

        /// <summary>
        /// Initializes the NPC and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            void startComponent(Type type)
            {
                EntityComponent c = _components.FirstOrDefault(cm => cm.GetType().IsSubclassOf(type));
                if(c != null)
                c.Start();
            }

            startComponent(typeof(SoundComponent));
            startComponent(typeof(InputComponent));
            startComponent(typeof(PhysicsComponent));
            startComponent(typeof(AIComponent));
        }

        /// <summary>
        /// Processes the LocalityChanged message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityChanged(LocalityChanged message)
=> _locality = message.Target;

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();

            foreach (EntityComponent c in _components)
                c.Update();
        }

        /// <summary>
        /// Destroys the NPC.
        /// </summary>
        protected override void Destroy()
        {
            Locality.TakeMessage(new LocalityLeft(this, this, Locality));
            World.Remove(this);
        }

        /// <summary>
        /// Processes the PositionChanged message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnPositionChanged(PositionChanged message)
        {
            _area = message.TargetPosition; // Set new position.
            _locality = message.TargetLocality;

            // Record visited locality.
            if (message.SourceLocality != null && message.SourceLocality != message.TargetLocality)
                RecordLocality(message.SourceLocality);

            // Inform all adjecting localities
            List<Locality> localities = new List<Locality>();
            if (message.SourceLocality!= null && message.SourceLocality != message.TargetLocality)
            {
                localities.Add(message.SourceLocality);
                localities.AddRange(message.SourceLocality.Neighbours);
            }

            if (message.TargetLocality!= null)
            {
                localities.Add(message.TargetLocality);
                localities.AddRange(message.TargetLocality.Neighbours);
            }

            IEnumerable<Locality> targetLocalities = localities.Distinct<Locality>();

            EntityMoved moved = new EntityMoved(this, message.SourcePosition, message.TargetPosition, message.SourceLocality, message.TargetLocality);
            LocalityLeft left = new LocalityLeft(this, this, message.SourceLocality);
            LocalityEntered entered = new LocalityEntered(this, this, message.TargetLocality);

            foreach (Locality l in targetLocalities)
            {
                l.TakeMessage(moved);

                    if(message.SourceLocality != null && message.SourceLocality != message.TargetLocality)
                    l.TakeMessage(left);

                    if(message.SourceLocality != message.TargetLocality)
                    l.TakeMessage(entered);
            }
        }

        /// <summary>
        /// Records the current locality as visited.
        /// </summary>
        protected void RecordLocality(Locality locality)
        {
            if (!VisitedLocalities.Contains(locality))
                _visitedLocalities.Add(locality.Name.Indexed);
        }

        /// <summary>
        /// Sends a message to al components.
        /// </summary>
        /// <param name="message">Message to redistribute</param>
        protected virtual void SendInnerMessage(GameMessage message)
        {
            foreach(EntityComponent c in _components)
            {
                if (c != message.Sender || message is GameReloaded)
                    c.TakeMessage(message);
            }
        }

        /// <summary>
        /// Checks if a message came from inside the NPC.
        /// </summary>
        /// <param name="message">The message to check</param>
        /// <returns>True if the message came from inside the NPC</returns>
        private bool IsInternal(GameMessage message)
            => message.Sender is EntityComponent c && c.Owner == this;

        /// <summary>
        /// Processes the Destroy message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private new void OnDestroy(Destroy message)
        {
            Assert(IsInternal(message), "This message can be sent only from an inner component.");
            Destroy();
        }
    }
}