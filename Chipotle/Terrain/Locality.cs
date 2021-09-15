using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using Game.Entities;
using Game.Messaging;
using Game.Messaging.Events;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    /// <summary>
    /// Defines a locality object e.g. a room or some place outside like meadow, park or yard.
    /// </summary>
    public class Locality : MapElement
    {
        /// <summary>
        /// Height of ceiling (is 0 in case of outdoor localities)
        /// </summary>
        public readonly int Ceiling;

        /// <summary>
        /// All exits from the locality
        /// </summary>
        public readonly IReadOnlyList<Passage> Passages;

        /// <summary>
        /// Specifies if the locality is a room or an outdoor place
        /// </summary>
        public readonly LocalityType Type;

        protected readonly string _backgroundSound;
        protected int _backgroundSoundId;
        private const int MinimumHeight = 3;
        private const int MinimumWidth = 3;
        private int _ceiling;
        private List<Entity> _entities;

        private List<GameObject> _objects;

        private List<Passage> _passages;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the locality</param>
        /// <param name="type">Is it a rom or an outdoor locality?</param>
        /// <param name="defaultTerrain">Lowest layer of the terrain</param>
        /// <param name="ceiling">Ceiling height (should be 0 for outdoor localities)</param>
        /// <param name="area">Area occupied with the locality</param>
        public Locality(Name name, LocalityType type, int ceiling, Plane area, TerrainType defaultTerrain, string backgroundSound = null) : base(name, area)
        {
            Type = type;
            _ceiling = Type == LocalityType.Outdoor && ceiling <= 2 ? 0 : ceiling;
            _ceiling = type == LocalityType.Outdoor ? 0 : ceiling;
            _passages = new List<Passage>();
            Passages = _passages.AsReadOnly();
            _entities = new List<Entity>();
            Entities = _entities.AsReadOnly();
            _objects = new List<GameObject>();
            Objects = _objects.AsReadOnly();
            DefaultTerrain = defaultTerrain;
            Area.MinimumHeight = MinimumHeight;
            Area.MinimumWidth = MinimumWidth;
            _backgroundSound = backgroundSound;

            Appear();
        }

        /// <summary>
        /// Defines the lowest layer of terrain
        /// </summary>
        public TerrainType DefaultTerrain { get; }

        public ReadOnlyCollection<Entity> Entities { get; }

        public ReadOnlyCollection<GameObject> Objects { get; }

        /// <summary>
        /// Draws walls around the locality
        /// </summary>
        /// <param name="walls">Specifies which walls should be drawn</param>
        public void DrawWalls(string walls)
        {
            IEnumerable<Vector2> wallCoordinates;

            if (walls == "All")
                wallCoordinates = Area.GetPerimeterPoints();
            else
            {
                List<Direction> directions = new List<Direction>();

                foreach (string word in Regex.Split(walls, @", +"))
                {
                    switch (word.ToLower())
                    {
                        case "left": directions.Add(Direction.Left); break;
                        case "front": directions.Add(Direction.Up); break;
                        case "right": directions.Add(Direction.Right); break;
                        case "back": directions.Add(Direction.Down); break;
                        default: throw new ArgumentException(nameof(word)); break;
                    }
                }

                wallCoordinates = Area.GetPerimeterPoints(directions);
            }

            wallCoordinates.Foreach(c => World.Map[c].Register(TerrainType.Wall, false));
        }

        /// <summary>
        /// Checks if a game object is present in this locality in the moment.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>True if the object is here</returns>
        public bool IsItHere(GameObject o) => _objects.Contains(o);

        /// <summary>
        /// Checks if an entity is present in this locality in the moment.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>True if the entity is here</returns>
        public bool IsItHere(Entity e) => _entities.Contains(e);

        public bool IsItHere(Passage p) => _passages.Contains(p);

        /// <summary>
        /// All neighbour localities in given direction set.
        /// </summary>
        /// <param name="directions">wanted directions</param>
        /// <returns>locality list</returns>
        public IEnumerable<Locality> NeighbourLocalities(Directions directions) =>
            //todo Location.Neighbours
            throw new NotImplementedException();

        public override void ReceiveMessage(GameMessage message)
        {
            base.ReceiveMessage(message);

            MessageObjects(message);
            MessageEntities(message);
        }

        public void Register(Passage p)
        {
            // Check if exit isn't already in list
            Assert(!IsItHere(p), "exit already registered");
            _passages.Add(p);

            if (_messagingEnabled)
                ReceiveMessage(new PassageShown(this, p));
        }

        /// <summary>
        /// Adds a game object to list of present objects.
        /// </summary>
        /// <param name="o">The object ot be added</param>
        public void Register(GameObject o)
        {
            Assert(!IsItHere(o), "Object already registered.");

            _objects.Add(o);
            if (_messagingEnabled)
            {
                if (_messagingEnabled)
                {
                    ReceiveMessage(new ObjectShown(this, o));
                }
            }
        }

        /// <summary>
        /// Adds an entity to list of present entities.
        /// </summary>
        /// <param name="e">The entity ot be added</param>
        public void Register(Entity e)
        {
            Assert(!IsItHere(e), "Entity already registered.");
            _entities.Add(e);
        }

        public override void Start()
        {
            base.Start();

            RegisterMessages(new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(LocalityEntered)] = (m) => OnLocalityEntered((LocalityEntered)m),
                [typeof(LocalityLeft)] = (m) => OnLocalityLeft((LocalityLeft)m)
            });
        }

        /// <summary>
        /// Immediately removes a game object from list of present objects.
        /// </summary>
        /// <param name="o"></param>
        public void Unregister(GameObject o)
        {
            _objects.Remove(o);
            ReceiveMessage(new ObjectHidden(this, o));
        }

        /// <summary>
        /// Immediately removes an entity from list of present entities.
        /// </summary>
        /// <param name="e">The entity to be removed</param>
        public void Unregister(Entity e)
        {
            _entities.Remove(e);
            ReceiveMessage(new EntityHidden(this, e));
        }

        public void Unregister(Passage p)
        {
            _passages.Remove(p);
            ReceiveMessage(new PassageDisappearedMessage(this, p));
        }

        protected override void Appear()
        {
            foreach (Vector2 point in Area.GetPoints())
                World.Map[point] = World.Map[point] == null ? new Tile(DefaultTerrain, point, this) : throw new InvalidOperationException("Tile must be empty");
        }

        protected override void Disappear()
        {
            if (!_objects.IsNullOrEmpty())
                new List<GameObject>(_objects).ForEach(o => World.Remove(o));

            if (!_passages.IsNullOrEmpty())
                new List<Passage>(_passages).ForEach(p => World.Remove(p));

            if (!_entities.IsNullOrEmpty())
                new List<Entity>(_entities).ForEach(e => World.Remove(e));

            Area.GetPoints().Foreach(p => World.Map[p] = null);
        }

        protected void MessageObjects(GameMessage message)
=> _objects.ForEach(o => o.ReceiveMessage(message));

        /// <summary>
        /// Checks if
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected Locality NeighbourLocality(Direction direction) =>
            //todo NeighbourLocality
            throw new NotImplementedException();

        private void MessageEntities(GameMessage message)
                => _entities.ForEach(e => e.ReceiveMessage(message));

        private void OnLocalityEntered(LocalityEntered message)
        {
            Register(message.Sender as Entity);

            if (message.Entity == World.Player && !string.IsNullOrEmpty(_backgroundSound))
                _backgroundSoundId = World.Sound.Play(World.Sound.GetSoundStream(_backgroundSound), null, true, PositionType.None, Vector3.Zero, false, 1, null, 1, 0, Playback.OpenAL);

            _entities.ForEach(e => e.ReceiveMessage(message));
            _objects.ForEach(o => o.ReceiveMessage(message));
            _passages.ForEach(p => p.ReceiveMessage(message));
        }

        private void OnLocalityLeft(LocalityLeft message)
        {
            Unregister(message.Sender as Entity);
            if (message.Sender == World.Player && _backgroundSoundId > 0)
                World.Sound.Stop(_backgroundSoundId);
        }
    }
}