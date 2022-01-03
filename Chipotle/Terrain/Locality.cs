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
        /// Checks if the specified point lays in front or behind a passage.
        /// </summary>
        /// <param name="point">The point to be checked</param>
        /// <returns>The passage by in front or behind which the specified point lays or null if nothing found</returns>
        public Passage IsAtPassage(Vector2 point)
            => Passages.FirstOrDefault(p => p.IsInFrontOrBehind(point));

        /// <summary>
        /// Checks if the specified entity is in any neighbour locality.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
        public Locality IsInNeighbourLocality(Entity e)
            => Neighbours.FirstOrDefault(l => l.IsItHere(e));

        /// <summary>
        /// Checks if the specified object is in any neighbour locality.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
        public Locality IsInNeighbourLocality(DumpObject o)
            => Neighbours.FirstOrDefault(l => l.IsItHere(o));

        /// <summary>
        /// Checks if the specified entity is in any accessible neighbour locality.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
        public Locality IsInAccessibleLocality(Entity e)
            => GetAccessibleLocalities().FirstOrDefault(l => l.IsItHere(e));

        /// <summary>
        /// Checks if the specified object is in any accessible neighbour locality.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
        public Locality IsInAccessibleLocality(DumpObject o)
            => GetAccessibleLocalities().FirstOrDefault(l => l.IsItHere(o));

        /// <summary>
        /// Returns all open passages.
        /// </summary>
        /// <returns>Enumeration of all open passages</returns>
        public IEnumerable<Passage> GetApertures()
            => Passages.Where(p => p.State == PassageState.Open);

        /// <summary>
        /// Chekcs if the specified locality is accessible from this locality.
        /// </summary>
        /// <param name="l">The locality to be checked</param>
        /// <returns>True if the specified locality is accessible form this locality</returns>
        public bool IsAccessible(Locality l)
            => GetAccessibleLocalities().Any(locality => locality==l);

        /// <summary>
        /// Checks if the specified locality is next to this locality.
        /// </summary>
        /// <param name="l">The locality to be checked</param>
        /// <returns>True if the speicifed locality is adjecting to this locality</returns>
        public bool IsNeighbour(Locality l)
            => Neighbours.Contains(l);

        /// <summary>
        /// Maps all adejcting localities.
        /// </summary>
        private void FindNeighbours()
        {
            Plane a = Area;
            a.Extend();
            _neighbours = 
                (
                from p in a.GetPerimeterPoints()
                let l = World.GetLocality(p)
                where(l != null)
                select l
                ).Distinct().ToList<Locality>();

            Neighbours = _neighbours.AsReadOnly();
        }

        /// <summary>
        /// List of adjecting localities
        /// </summary>
        private List<Locality> _neighbours;

        /// <summary>
        /// List of adjecting localities
        /// </summary>
        public IReadOnlyList<Locality> Neighbours { get; private set; }
        /// <summary>
        /// Returns all open passages between this locality and the specified one..
        /// </summary>
        /// <returns>Enumeration of all open passages between this locality and the specified one</returns>
        public IEnumerable<Passage> GetApertures(Locality l)
            => GetApertures().Where(p => p.LeadsTo(l));

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
        /// <param name="backgroundInfo">A background sound played in loop</param>
        public Locality(Name name, string to, LocalityType type, int ceiling, Plane area, TerrainType defaultTerrain, string loop, float volume) : base(name, area)
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

            _loop = loop;
            _defaultVolume = volume;

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
        public bool IsItHere(DumpObject o) => _objects.Contains(o);

        /// <summary>
        /// Checks if an entity is present in this locality in the moment.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>True if the entity is here</returns>
        public bool IsItHere(Entity e)
            => e.Locality == this;

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
            MessagePassages(message);
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
        public void Register(DumpObject o)
        {
            Assert(!IsItHere(o), "Object already registered.");

            _objects.Add(o as DumpObject);
        }

        /// <summary>
        /// Adds an entity to locality.
        /// </summary>
        /// <param name="e">The entity to be added</param>
        public void Register(Entity e)
        {
            Assert(!_entities.Contains(e), "Entity already registered.");
            _entities.Add(e);
        }

        /// <summary>
        /// Initializes the locality and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();
            FindNeighbours();

            RegisterMessages(new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(DoorManipulated)] = (message) => OnDoorManipulated((DoorManipulated)message),
                [typeof(GameReloaded)] = (message) => OnGameReloaded(),
                [typeof(LocalityEntered)] = (m) => OnLocalityEntered((LocalityEntered)m),
                [typeof(LocalityLeft)] = (m) => OnLocalityLeft((LocalityLeft)m)
            });
        }

        /// <summary>
        /// Handles the DoorManipulated message.
        /// </summary>
        /// <param name="message">Source of the message</param>
        private void OnDoorManipulated(DoorManipulated message)
            => UpdateLoop();

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
                UpdateLoop();
        }

        /// <summary>
        /// Handles the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message</param>
        private void OnLocalityEntered(LocalityEntered message)
        {
            if (message.Locality == this)
            Register(message.Sender as Entity);

                if (message.Entity == World.Player)
            {
                if (message.Locality == this)
                    _playerInHere = true;
                UpdateLoop();
            }
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
            if (message.Locality == this)
            Unregister(message.Sender as Entity);

            if (message.Entity == World.Player)
            {
                if(message.Locality == this)
                _playerInHere = false;
            }
        }

        /// <summary>
        /// Stores handles for all background sound instances.
        /// </summary>
        private List<int> _loops = new List<int>();

        /// <summary>
        /// Name of loop sound.
        /// </summary>
        private string _loop;

        /// <summary>
        /// Lowpass filter parameters used when player is standing out the locality.
        /// </summary>
        private readonly (float gain, float gainHF) _closedDoorLowpass = (1, .5f);

        /// <summary>
        /// Plays the background sound of this locality in a loop.
        /// </summary>
        /// <param name="playerMoved">Specifies if the player just moved from one locality to another one.</param>
        private void UpdateLoop()
        {
            if (_playerInHere)
                PlayLoop();
            else if (IsInAccessibleLocality(World.Player) != null)
                PlayLoop(false, true);
            else PlayLoop(false, false);
        }

        /// <summary>
        /// Plays soudn loop of the locality.
        /// </summary>
        /// <param name="playerInHere">Determines if the player is in this locality.</param>
        /// <param name="accessible">Determines if the player is in an neighbour accessible locality.</param>
        protected void PlayLoop(bool playerInHere = true, bool accessible = false)
        {
            if (string.IsNullOrEmpty(_loop)) // No loop
                return;

            StopBackground(); // The loop must be stopped everytime due to problems with fading effects.
            int id; // sound handle

            // Player is in the locality
            if (playerInHere)
            {
                id = World.Sound.Play(World.Sound.GetSoundStream(_loop), null, true, PositionType.None, Vector3.Zero, false, _defaultVolume);
                _loops.Add(id);
                return;
            }

            // Player is in a neighbour accessible locality 
            if (accessible)
            {
                foreach (Passage p in World.Player.Locality.GetPassagesTo(this))
                {
                    Vector2? tmp = World.Player.Area.FindOppositePoint(p.Area);
                    Vector2 position = tmp.HasValue ? (Vector2)tmp : p.Area.Center;
                    float volume = p.State == PassageState.Closed ? _defaultVolume : OverDoorVolume;
                    id = World.Sound.Play(World.Sound.GetSoundStream(_loop), null, true, PositionType.Absolute, position.AsOpenALVector(), true, volume);
                    _loops.Add(id);

                    if (p.State == PassageState.Closed)
                    {
                        World.Sound.ApplyLowpass(id, _overDoorLowpass);
                        World.Sound.FadeSource(id, FadingType.Out, .00005f, OverDoorVolume, false);
                    }
                    else
                        World.Sound.FadeSource(id, FadingType.In, .00005f, _defaultVolume, false);
                }
                return;
            }

            // Player is in a distant locality or behind a wall without doors.
            id = World.Sound.Play(World.Sound.GetSoundStream(_loop), null, true, PositionType.Absolute, _area.Center.AsOpenALVector(), true, OverWallVolume);
            _loops.Add(id);
            World.Sound.ApplyLowpass(id, _overDoorLowpass);
        }

        /// <summary>
        /// Returns all passages leading to the specified locality.
        /// </summary>
        /// <param name="locality">The locality to which the passages should lead</param>
        /// <returns> all passages leading to the specified locality</returns>
        private IEnumerable<Passage> GetPassagesTo(Locality locality)
            => Passages.Where(p => p.Localities.Contains(locality));

        //protected new float OverDoorVolume => .05f * _defaultVolume;


        /// <summary>
        /// Stops all isntances of background sound.
        /// </summary>
        private void StopBackground()
        {
            foreach (int id in _loops)
                World.Sound.FadeSource(id, FadingType.Out, .0002f, 0, true);

            _loops = new List<int>();
        }
    }
}