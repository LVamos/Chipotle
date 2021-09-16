using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    public class Entity : GameObject
    {
        protected AIComponent _ai;
        protected List<EntityComponent> _components;
        protected InputComponent _input;
        protected PhysicsComponent _physics;
        protected SoundComponent _sound;
        protected HashSet<Locality> _visitedLocalities = new HashSet<Locality>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Entity(Name name, string type, AIComponent ai, InputComponent input, PhysicsComponent physics, SoundComponent sound) : base(name, type, null)
        {
            _physics = physics;
            _sound = sound;
            _ai = ai;
            _input = input;
            _components = (new EntityComponent[] { ai, input, sound, physics }).Where(c => c != null).ToList<EntityComponent>();

            _components.ForEach(c => c.Owner = this);
        }

        public Orientation2D Orientation => _physics.Orientation;
        public IReadOnlyCollection<Locality> VisitedLocalities => _visitedLocalities;

        public static Entity CreateBartender()
            => new Entity(new Name("Bartender", "pingl"), "Bartender", new BartenderAIComponent(), null, new PhysicsComponent(), new SoundComponent());

        public static Entity CreateCarson()
            => new Entity(new Name("Carson", "David Carson"), "Carson", new CarsonAIComponent(), null, new PhysicsComponent(), new SoundComponent());

        /// <summary>
        /// Creates the Columbo entity
        /// </summary>
        /// <returns>Instance of Columbo</returns>
        public static Entity CreateChipotle()
            => new Entity(new Name("Chipotle", "detektiv Chipotle"), "Chipotle", null, new ChipotleInputComponent(), new ChipotlePhysicsComponent(), new ChipotleSoundComponent());

        public static Entity CreateChristine()
            => new Entity(new Name("Christine", "Christine Piercová"), "Christine", new ChristineAIComponent(), null, new ChristinePhysicsComponent(), new SoundComponent());

        public static Entity CreateMariotti()
            => new Entity(new Name("Mariotti", "Paolo Mariotti"), "Mariotti", new MariottiAIComponent(), null, new PhysicsComponent(), new SoundComponent());

        public static Entity CreateSweeney()
            => new Entity(new Name("Sweeney", "Derreck Sweeney"), "Sweeney", new SweeneyAIComponent(), null, new PhysicsComponent(), new SoundComponent());

        public static Entity CreateTuttle()
            => new Entity(new Name("Tuttle", "parťák"), "Tuttle", new TuttleAIComponent(), null, new TuttlePhysicsComponent(), new TuttleSoundComponent());

        public override void ReceiveMessage(GameMessage message)
        {
            base.ReceiveMessage(message);

            if (_messagingEnabled)
                SendInnerMessage(message);
        }

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
        /// Updates state of all components and lets them react on other game objects.
        /// </summary>
        public override void Update()
        {
            base.Update();
            _components.ForEach(c => c.Update());
        }

        protected override void Destroy()
        {
            Locality?.Unregister(this);
            World.Remove(this);
            _area.GetTiles().Foreach(t => t.UnregisterObject());
        }

        protected void OnPositionChanged(PositionChanged m)
        {
            _area = m.Area;
            RecordLocality();
            _ai?.ReceiveMessage(m);
        }

        protected void RecordLocality()
        {
            Locality locality = _area.GetLocality();
            if (!VisitedLocalities.Contains(locality))
                _visitedLocalities.Add(locality);
        }

        /// <summary>
        /// Sends message to al components.
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

        private bool IsInternal(GameMessage message)
            => message.Sender is EntityComponent c && c.Owner == this;

        private void OnDestroy(Destroy message)
        {
            Assert(IsInternal(message), "This message can be sent only from an inner component.");
            Destroy();
        }
    }
}