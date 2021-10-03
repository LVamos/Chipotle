using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Represents an NPC.
    /// </summary>
    public class Entity : GameObject
    {
        /// <summary>
        /// Reference to a component that controls behavior of the NPC
        /// </summary>
        protected AIComponent _ai;

        /// <summary>
        /// List of all entity components
        /// </summary>
        protected List<EntityComponent> _components;

        /// <summary>
        /// Reference to a component that processes input from the player
        /// </summary>
        protected InputComponent _input;

        /// <summary>
        /// Reference to a component that controls movement of the NPC
        /// </summary>
        protected PhysicsComponent _physics;

        /// <summary>
        /// Reference to a component that controls sound output of the NPC
        /// </summary>
        protected SoundComponent _sound;

        /// <summary>
        /// List of all localities visited by the NPC
        /// </summary>
        protected HashSet<Locality> _visitedLocalities = new HashSet<Locality>();

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
            _physics = physics;
            _sound = sound;
            _ai = ai;
            _input = input;
            _components = (new EntityComponent[] { ai, input, sound, physics }).Where(c => c != null).ToList<EntityComponent>();

            _components.ForEach(c => c.Owner = this);
        }

        /// <summary>
        /// Returns the current orientation of the NPC in the game world.
        /// </summary>
        public Orientation2D Orientation => _physics.Orientation;

        /// <summary>
        /// List of all localities visited by the NPC
        /// </summary>
        public IReadOnlyCollection<Locality> VisitedLocalities => _visitedLocalities;

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
        public override void ReceiveMessage(GameMessage message)
        {
            base.ReceiveMessage(message);

            if (_messagingEnabled)
                SendInnerMessage(message);
        }

        /// <summary>
        /// Initializes the NPC and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();
            _sound?.Start();
            _input?.Start();
            _physics?.Start();
            _ai?.Start();

            RegisterMessages(
                new Dictionary<System.Type, System.Action<GameMessage>>
                {
                    [typeof(PositionChanged)] = (m) => OnPositionChanged((PositionChanged)m),
                }
                );
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            _components.ForEach(c => c.Update());
        }

        /// <summary>
        /// Destroys the NPC.
        /// </summary>
        protected override void Destroy()
        {
            Locality?.Unregister(this);
            World.Remove(this);
            _area.GetTiles().Foreach(t => t.UnregisterObject());
        }

        /// <summary>
        /// Processes the PositionChanged message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnPositionChanged(PositionChanged message)
        {
            _area = message.TargetPosition;

            Locality source = message.SourcePosition?.GetLocality();
            Locality target = message.TargetPosition.GetLocality();

            if (source != null && source != target)
                RecordLocality(source);

            _ai?.ReceiveMessage(message);
        }

        /// <summary>
        /// Records the current locality as visited.
        /// </summary>
        protected void RecordLocality(Locality locality)
        {
            if (!VisitedLocalities.Contains(locality))
                _visitedLocalities.Add(locality);
        }

        /// <summary>
        /// Sends a message to al components.
        /// </summary>
        /// <param name="message">Message to redistribute</param>
        protected virtual void SendInnerMessage(GameMessage message)
        {
            List<EntityComponent> targetComponents = new List<EntityComponent>();
            targetComponents = _components.Where(c => c != message.Sender).ToList<EntityComponent>();
            if (!IsInternal(message) && !(message is CutsceneBegan) && !(message is CutsceneEnded))
                targetComponents.Remove(_sound);

            targetComponents.Foreach(c => c.ReceiveMessage(message));
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