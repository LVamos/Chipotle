using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using DavyKager;

using Game.Entities;
using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    /// <summary>
    /// Represents one region on the game map (e.g. a room).
    /// </summary>
    [Serializable]
    public class Locality : MapElement
    {

        /// <summary>
        /// Enumerates passages ordered by distance from the specified point.
        /// </summary>
        /// <param name="point">The default point</param>
        /// <param name="radius">distance in which exits from current locality are searched</param>
        /// <returns>Enumeration of passages</returns>
        public IEnumerable<Passage> GetNearestExits(Vector2 point, int radius)
        {
            return
                from e in Passages
                where(!e.Area.LaysOnPlane(point) && World.GetDistance(e.Area.GetClosestPointTo(point), point) <=radius)
                orderby e.Area.GetDistanceFrom(point)
                select e;
        }

        /// <summary>
        /// Enumerates all dump objects around the specified <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point in whose surroundings the objects should be listed.</param>
        /// <param name="radius">Max distance from the speciifed <paramref name="point"/></param>
        /// <param name="decorative">Specifies if the method lists decorative objects such as fences or rails.</param>
        /// <returns>Enumeration of dump objectts</returns>
        public IEnumerable<DumpObject> GetSurroundingObjects(Vector2 point, int radius, bool decorative = false)
        {
            return
                from o in Objects
                where (o.Decorative == decorative && o.Area.GetDistanceFrom(point) <= radius)
                orderby o.Area.GetDistanceFrom(point)
                select o;
        }

        /// <summary>
        /// Height of ceiling of the locality (0 in case of outdoor localities)
        /// </summary>
        public readonly int Ceiling;

        /// <summary>
        /// All exits from the locality
        /// </summary>
        public readonly IReadOnlyList<Passage> Passages;

        /// <summary>
        /// Name of the locality in a shape that expresses a direction to the locality.
        /// </summary>
        public readonly string To;

        /// <summary>
        /// Specifies if the locality is outside or inside a building.
        /// </summary>
        public readonly LocalityType Type;

        /// <summary>
        /// Name of a background sound played in loop when the Detective Chipotle NPC is inside
        /// </summary>
        protected readonly string _backgroundSound;

        /// <summary>
        /// Handle of a background sound played in loop when the Detective Chipotle NPC is inside
        /// </summary>
        protected int _backgroundSoundId;
        private int _backgroundSoundIdTemp;
        private bool _playerNearby;

        /// <summary>
        /// The minimum permitted Y dimension of the floor in this locality
        /// </summary>
        private const int MinimumHeight = 3;

        /// <summary>
        /// The minimum permitted X dimension of the floor in this locality
        /// </summary>
        private const int MinimumWidth = 3;

        /// <summary>
        /// Height of ceiling of the locality (0 in case of outdoor localities)
        /// </summary>
        private readonly int _ceiling;

        /// <summary>
        /// List of NPCs present in this locality.
        /// </summary>
        private readonly List<Entity> _entities;

        /// <summary>
        /// List of objects present in this locality.
        /// </summary>
        private readonly List<DumpObject> _objects;

        /// <summary>
        /// List of exits from this locality
        /// </summary>
        private readonly List<Passage> _passages;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name of the locality</param>
        /// <param name="to">
        /// Name of the locality in a shape that expresses a direction to the locality
        /// </param>
        /// <param name="type">Specifies if the locality is outside or inside a building.</param>
        /// <param name="ceiling">Ceiling height of the locality (should be 0 for outdoor localities)</param>
        /// <param name="area">Coordinates of the area occupied by the locality</param>
        /// <param name="defaultTerrain">Lowest layer of the terrain in the locality</param>
        /// <param name="backgroundSound">A background sound played in loop</param>
        public Locality(Name name, string to, LocalityType type, int ceiling, Plane area, TerrainType defaultTerrain, string backgroundSound = null) : base(name, area)
        {
            To = to;
            Type = type;
            _ceiling = Type == LocalityType.Outdoor && ceiling <= 2 ? 0 : ceiling;
            _ceiling = type == LocalityType.Outdoor ? 0 : ceiling;
            _passages = new List<Passage>();
            Passages = _passages.AsReadOnly();
            _entities = new List<Entity>();
            Entities = _entities.AsReadOnly();
            _objects = new List<DumpObject>();
            Objects = _objects.AsReadOnly();
            DefaultTerrain = defaultTerrain;
            Area.MinimumHeight = MinimumHeight;
            Area.MinimumWidth = MinimumWidth;
            _backgroundSound = backgroundSound;

            Appear();
        }

        /// <summary>
        /// Defines the lowest layer of terrain in the locality.
        /// </summary>
        public TerrainType DefaultTerrain { get; }

        /// <summary>
        /// List of NPCs present in this locality.
        /// </summary>
        public ReadOnlyCollection<Entity> Entities { get; }

        /// <summary>
        /// List of objects present in this locality.
        /// </summary>
        public ReadOnlyCollection<DumpObject> Objects { get; }

        /// <summary>
        /// Returns all adjecting localities to which it's possible to get from this locality.
        /// </summary>
        /// <returns>An enumeration of all adjecting accessible localities</returns>
        public IEnumerable<Locality> GetAccessibleLocalities()
            => _passages.Select(p => p.AnotherLocality(this));

        /// <summary>
        /// Checks if the specified game object is present in this locality in the moment.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>True if the object is present in the locality</returns>
        public bool IsItHere(GameObject o) => _objects.Contains(o);

        /// <summary>
        /// Checks if an entity is present in this locality in the moment.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>True if the entity is here</returns>
        public bool IsItHere(Entity e) => _entities.Contains(e);

        /// <summary>
        /// Checks if a passage lays in the locality.
        /// </summary>
        /// <param name="p">The passage to be checked</param>
        /// <returns>True if the passage lays in this locality</returns>
        public bool IsItHere(Passage p) => _passages.Contains(p);

        /// <summary>
        /// Gets a message from another messaging object and stores it for processing.
        /// </summary>
        /// <param name="message">The message to be received</param>
        public override void ReceiveMessage(GameMessage message)
        {
            base.ReceiveMessage(message);

            MessageObjects(message);
            MessageEntities(message);
        }

        /// <summary>
        /// Adds an passage to the locality.
        /// </summary>
        /// <param name="p">The passage to be added</param>
        public void Register(Passage p)
        {
            // Check if exit isn't already in list
            Assert(!IsItHere(p), "exit already registered");
            _passages.Add(p);
        }

        /// <summary>
        /// Adds a game object to list of present objects.
        /// </summary>
        /// <param name="o">The object ot be added</param>
        public void Register(GameObject o)
        {
            Assert(!IsItHere(o), "Object already registered.");

            _objects.Add(o as DumpObject);
            if (_messagingEnabled)
            {
            }
        }

        /// <summary>
        /// Adds an entity to locality.
        /// </summary>
        /// <param name="e">The entity to be added</param>
        public void Register(Entity e)
        {
            Assert(!IsItHere(e), "Entity already registered.");
            _entities.Add(e);
        }

        /// <summary>
        /// Initializes the locality and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            RegisterMessages(new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(DoorManipulated)] = (message) => OnDoorManipulated((DoorManipulated)message),
                [typeof(GameReloaded)] = (message) => OnGameReloaded(),
                [typeof(LocalityEntered)] = (m) => OnLocalityEntered((LocalityEntered)m),
                [typeof(LocalityLeft)] = (m) => OnLocalityLeft((LocalityLeft)m)
            });

            PlayBackground();
        }

        /// <summary>
        /// Handles the DoorManipulated message.
        /// </summary>
        /// <param name="message">Source of the message</param>
        private void OnDoorManipulated(DoorManipulated message)
            => PlayBackground();

        /// <summary>
        /// Indicates if the player is in the locality.
        /// </summary>
        /// <returns>True if the player is in the locality.</returns>
        private bool _playerInHere;

        /// <summary>
        /// Immediately removes a game object from list of present objects.
        /// </summary>
        /// <param name="o"></param>
        public void Unregister(GameObject o)
            => _objects.Remove(o as DumpObject);

        /// <summary>
        /// Immediately removes an entity from list of present entities.
        /// </summary>
        /// <param name="e">The entity to be removed</param>
        public void Unregister(Entity e)
            => _entities.Remove(e);

        /// <summary>
        /// Removes a passage from the locality.
        /// </summary>
        /// <param name="p">The passage to be removed</param>
        public void Unregister(Passage p)
        {
            _passages.Remove(p);
        }

        /// <summary>
        /// Displays the locality in game world.
        /// </summary>
        protected override void Appear()
        {
            foreach (Vector2 point in Area.GetPoints())
                World.Map[point] = World.Map[point] == null ? new Tile(DefaultTerrain) : throw new InvalidOperationException("Tile must be empty");
        }

        /// <summary>
        /// Erases the locality from game world.
        /// </summary>
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

        /// <summary>
        /// Sends a message to all game objects in the locality.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        protected void MessageObjects(GameMessage message)
=> _objects.ForEach(o => o.ReceiveMessage(message));

        private void MessageEntities(GameMessage message)
                        => _entities.ForEach(e => e.ReceiveMessage(message));

        /// <summary>
        /// Handles the GameReloaded message.
        /// </summary>
        private void OnGameReloaded()
        {
            _backgroundSoundId = 0;
            _playerInHere = IsItHere(World.Player);
                PlayBackground();
        }

        /// <summary>
        /// Handles the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message</param>
        private void OnLocalityEntered(LocalityEntered message)
        {
            Register(message.Sender as Entity);

            _playerInHere = message.Entity== World.Player;
            PlayBackground(true);

            MessageEntities(message);
            MessageObjects(message);
            MessagePassages(message);
        }

        /// <summary>
        /// Distributes a game message to all passages.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        private void MessagePassages(GameMessage message)
        {
            foreach (Passage p in _passages)
                p.ReceiveMessage(message);
        }

        /// <summary>
        /// Handles the LocalityLeft message.
        /// </summary>
        /// <param name="message">The message</param>
        private void OnLocalityLeft(LocalityLeft message)
        {
            Unregister(message.Sender as Entity);
            _playerInHere = IsItHere(World.Player);
                PlayBackground(true);

            MessageEntities(message);
            MessageObjects(message);
            MessagePassages(message);
        }

        /// <summary>
        /// Stores handles for all background sound instances.
        /// </summary>
        private List<int> _backgroundSounds = new List<int>();

        /// <summary>
        /// Lowpass filter parameters used when player is standing out the locality.
        /// </summary>
        private readonly (float gain, float gainHF) _closedDoorLowpass = (1, .5f);

        /// <summary>
        /// Default volume of the background sound used when the player is inside.
        /// </summary>
        private const float _backgroundVolumeInside = 1;

        /// <summary>
        /// Plays the background sound of this locality in a loop.
        /// </summary>
        /// <param name="playerMoved">Specifies if the player just moved from one locality to another one.</param>
        private void PlayBackground(bool playerMoved=false)
        {
            if (string.IsNullOrEmpty(_backgroundSound))
                return;

            int id;
            if (!_playerInHere)
            {
            StopBackground();
                foreach (Passage p in _passages)
                {
                    Vector2 position;

                    if (World.Player == null || World.Player.Area == null)
                        position = p.Area.Center;
                    else
                    {
                        Vector2 player = World.Player.Area.Center;
                        position =
                        (
                            from point in p.Area.GetPoints()
                            where (point.X == player.X || point.Y == player.Y)
                            select (point)
                            ).FirstOrDefault();

                        if (position == default(Vector2))
                            position = p.Area.Center;
                    }

                    id = World.Sound.Play(World.Sound.GetSoundStream(_backgroundSound), null, true, PositionType.Absolute, position.AsOpenALVector(), true, _backgroundVolumeInside, null, 1, 0, Playback.OpenAL);
                    _backgroundSounds.Add(id);

                    if (p is Door d && d.State == Door.DoorState.Closed)
                        World.Sound.ApplyLowpass(id, _closedDoorLowpass.gain, _closedDoorLowpass.gainHF);
                }
                return;
            }

            // Player is inside.
            if (playerMoved)
            {
            StopBackground();
                id = World.Sound.Play(World.Sound.GetSoundStream(_backgroundSound), null, true, PositionType.None, Vector3.Zero, false, _backgroundVolumeInside, null, 1, 0, Playback.OpenAL);
                _backgroundSounds.Add(id);
            }
        }

        /// <summary>
        /// Stops all isntances of background sound.
        /// </summary>
        private void StopBackground()
        {
            foreach (int id in _backgroundSounds)
                World.Sound.FadeSource(id, FadingType.Out, .0002f, 0, true);

            _backgroundSounds = new List<int>();
        }
    }
}