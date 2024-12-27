using Game.Entities;
using Game.Messaging;
using Game.Messaging.Events;

using Luky;

using OpenTK;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// Represents one region on the game map (e.g. a room).
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class Locality : MapElement
    {
        /// <summary>
        /// Enumerates all passages leading to the specified locality.
        /// </summary>
        /// <param name="l">The target locality</param>
        /// <returns>enumaretion of passages</returns>
        public IEnumerable<Passage> GetExitsTo(Locality l)
            => Passages.Where(p => p.LeadsTo(l));

        /// <summary>
        /// Indicates if a locality is inside a building or outside.
        /// </summary>
        public enum LocalityType
        {
            /// <summary>
            /// A room or corridor in a building
            /// </summary>
            Indoor,

            /// <summary>
            /// An openair place like yard or meadow
            /// </summary>
            Outdoor
        }

        /// <summary>
        /// Enumerates all accessible localities.
        /// </summary>
        /// <returns>All accessible localities</returns>
        public IEnumerable<Locality> GetAccessibleLocalities()
                        => Passages.Select(p => p.AnotherLocality(this)).Distinct();



        /// <summary>
        /// Handles the EntityMoved message.
        /// </summary>
        /// <param name="message">The mesage to be handled</param>
        private void OnCharacterMoved(CharacterMoved message)
        {
            if (message.Sender == World.Player)
                UpdatePassageLoops();
        }

        /// <summary>
        /// Updates position of passage sound loops.
        /// </summary>
        private void UpdatePassageLoops()
        {
            if (_passageLoops.IsNullOrEmpty())
                return;

            foreach (Passage p in _passageLoops.Keys)
            {
                Vector2 player = World.Player.Area.Center;

                // If the palyer is standing right in the passage locate the sound right on his position.
                if (p.Area.Intersects(player))
                {
                    Locality other = p.AnotherLocality(World.Player.Locality);
                    World.Sound.SetSourcePosition(_passageLoops[p], other.Area.GetClosestPoint(player).AsOpenALVector());
                    continue;
                }

                // Try find a point of the passage which lays in opposite to the player.
                World.Sound.SetSourcePosition(_passageLoops[p], p.Area.GetClosestPoint(player).AsOpenALVector());
            }
        }

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
        public Locality IsInNeighbourLocality(Character e)
            => Neighbours.FirstOrDefault(l => l.IsItHere(e));

        /// <summary>
        /// Checks if the specified object is in any neighbour locality.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
        public Locality IsInNeighbourLocality(Item o)
            => Neighbours.FirstOrDefault(l => l.IsItHere(o));

        /// <summary>
        /// Checks if the specified entity is in any accessible neighbour locality.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
        public Locality IsInAccessibleLocality(Character e)
            => GetLocalitiesBehindDoor().FirstOrDefault(l => l.IsItHere(e));

        /// <summary>
        /// Checks if the specified object is in any accessible neighbour locality.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>An instance of the locality in which the specified entity is located or null if it wasn't found</returns>
        public Locality IsInAccessibleLocality(Item o)
            => GetLocalitiesBehindDoor().FirstOrDefault(l => l.IsItHere(o));

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
        public bool IsBehindDoor(Locality l)
            => GetLocalitiesBehindDoor().Any(locality => locality.Name.Indexed == l.Name.Indexed);

        /// <summary>
        /// Checks if it's possible to get to the specified locality from this locality over doors or open passages.
        /// </summary>
        /// <param name="locality">The target locality</param>
        /// <returns>True if there's a way between this loclaity and the specified locality</returns>
        public bool IsAccessible(Locality locality)
            => GetAccessibleLocalities().Any(l => l.Name.Indexed == locality.Name.Indexed);


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
            Rectangle a = Area;
            a.Extend();
            _neighbours =
                (
                from p in a.GetPerimeterPoints()
                let l = World.GetLocality(p)
                where (l != null)
                select l.Name.Indexed
                ).Distinct().ToList<string>();
        }

        /// <summary>
        /// List of adjecting localities
        /// </summary>
        private List<string> _neighbours;

        /// <summary>
        /// List of adjecting localities
        /// </summary>
        [ProtoIgnore]
        public IEnumerable<Locality> Neighbours => _neighbours.Select(n => World.GetLocality(n));

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
                where (!e.Area.Intersects(point) && World.GetDistance(e.Area.GetClosestPoint(point), point) <= radius)
                orderby e.Area.GetDistanceFrom(point)
                select e;
        }

        /// <summary>
        /// Enumerates all dump objects around the specified <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point in whose surroundings the objects should be listed.</param>
        /// <param name="radius">Max distance from the speciifed <paramref name="point"/></param>
        /// <param name="includeDecoration">Specifies if the method lists decorative objects such as fences or rails.</param>
        /// <returns>Enumeration of dump objectts</returns>
        public IEnumerable<Item> GetNearByObjects(Vector2 point, int radius, bool includeDecoration = false)
        {
            return
                from o in Objects
                where o.Area != null && o.Decorative == includeDecoration && o.Area.GetDistanceFrom(point) <= radius
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
        [ProtoIgnore]
        public IEnumerable<Passage> Passages
        {
            get
            {
                if (_passages == null)
                    _passages = new HashSet<string>();

                return _passages.Select(p => World.GetPassage(p)).Where(p => p != null != null);
            }
        }

        /// <summary>
        /// Text description of the locality
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Name of the locality in a shape that expresses a direction to the locality.
        /// </summary>
        public readonly string To;

        /// <summary>
        /// Specifies if the locality is outside or inside a building.
        /// </summary>
        public readonly LocalityType Type;

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
        private HashSet<string> _characters = new HashSet<string>();

        /// <summary>
        /// List of objects present in this locality.
        /// </summary>
        private HashSet<string> _objects = new HashSet<string>();

        /// <summary>
        /// List of exits from this locality
        /// </summary>
        private HashSet<string> _passages = new HashSet<string>();

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
        public Locality(Name name, string description, string to, LocalityType type, int ceiling, Rectangle area, TerrainType defaultTerrain, string loop, float volume) : base(name, area)
        {
            Description = description;
            To = to;
            Type = type;
            _ceiling = Type == LocalityType.Outdoor && ceiling <= 2 ? 0 : ceiling;
            _ceiling = type == LocalityType.Outdoor ? 0 : ceiling;
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
        [ProtoIgnore]
        public TerrainType DefaultTerrain { get; }

        /// <summary>
        /// List of NPCs present in this locality.
        /// </summary>
        [ProtoIgnore]
        public IEnumerable<Character> Characters => _characters.Select(c => World.GetCharacter(c));

        /// <summary>
        /// List of objects present in this locality.
        /// </summary>
        [ProtoIgnore]
        public IEnumerable<Item> Objects
        {
            get
            {
                if (_objects == null)
                    _objects = new HashSet<string>();

                return _objects.Select(o => World.GetObject(o)).Where(o => o != null);
            }
        }

        /// <summary>
        /// Returns all adjecting localities to which it's possible to get from this locality.
        /// </summary>
        /// <returns>An enumeration of all adjecting accessible localities</returns>
        public IEnumerable<Locality> GetLocalitiesBehindDoor()
            => Passages.Select(p => p.AnotherLocality(this));

        /// <summary>
        /// Checks if the specified game object is present in this locality in the moment.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>True if the object is present in the locality</returns>
        public bool IsItHere(Item o)
            => _objects.Contains(o.Name.Indexed);

        /// <summary>
        /// Checks if an entity is present in this locality in the moment.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>True if the entity is here</returns>
        public bool IsItHere(Character e)
            => Characters.Contains(e);

        /// <summary>
        /// Checks if a passage lays in the locality.
        /// </summary>
        /// <param name="p">The passage to be checked</param>
        /// <returns>True if the passage lays in this locality</returns>
        public bool IsItHere(Passage p) => Passages.Contains(p);

        /// <summary>
        /// Gets a message from another messaging object and stores it for processing.
        /// </summary>
        /// <param name="message">The message to be received</param>
        /// <param name="routeToNeighbours">Specifies if the message should be distributed to the neighbours of this locality</param>
        public void TakeMessage(GameMessage message, bool routeToNeighbours)
        {
            TakeMessage(message);

            if (routeToNeighbours)
            {
                foreach (Locality neighbour in Neighbours)
                    neighbour.TakeMessage(message);
            }
        }

        /// <summary>
        /// Gets a message from another messaging object and stores it for processing.
        /// </summary>
        /// <param name="message">The message to be received</param>
        public override void TakeMessage(GameMessage message)
        {
            base.TakeMessage(message);

            if (message is ChipotlesCarMoved)
                return; // Don't send this to other objects and entities.

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
            _passages.Add(p.Name.Indexed);
        }

        /// <summary>
        /// Adds a game object to list of present objects.
        /// </summary>
        /// <param name="o">The object ot be added</param>
        private void Register(Item o) => _objects.Add(o.Name.Indexed);

        /// <summary>
        /// Adds an entity to locality.
        /// </summary>
        /// <param name="c">The entity to be added</param>
        public void Register(Character c)
        {
            _characters.Add(c.Name.Indexed);
        }

        /// <summary>
        /// Initializes the locality and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();
            FindNeighbours();
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case ObjectDisappearedFromLocality m: OnObjectDisappearedFromLocality(m); break;
                case ObjectAppearedInLocality m: OnObjectAppearedInLocality(m); break;
                case ChipotlesCarMoved ccmv: OnChipotlesCarMoved(ccmv); break;
                case CharacterMoved em: OnCharacterMoved(em); break;
                case DoorManipulated dm: OnDoorManipulated(dm); break;
                case GameReloaded gr: OnGameReloaded(); break;
                case CharacterLeftLocality ll: OnCharacterLeftLocality(ll); break;
                case CharacterCameToLocality le: OnCharacterCameToLocality(le); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="m">The message to be handled</param>
        private void OnObjectDisappearedFromLocality(ObjectDisappearedFromLocality m) => Unregister(m.Object);

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="m">The message to be handled</param>
        private void OnObjectAppearedInLocality(ObjectAppearedInLocality m) => Register(m.Object);

        /// <summary>
        /// Handles the ChipotlesCarMoved message.
        /// </summary>
        /// <param name="message"The message to be handled></param>
        private void OnChipotlesCarMoved(ChipotlesCarMoved message)
        {
            if (message.Target.GetLocalities() != this)
                StopBackground(true);
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
        [ProtoIgnore]
        private bool _playerInHere;

        /// <summary>
        /// Immediately removes a game object from list of present objects.
        /// </summary>
        /// <param name="o"></param>
        private void Unregister(GameObject o)
            => _objects.Remove(o.Name.Indexed);

        /// <summary>
        /// Immediately removes an entity from list of present entities.
        /// </summary>
        /// <param name="e">The entity to be removed</param>
        public void Unregister(Character e)
            => _characters.Remove(e.Name.Indexed);

        /// <summary>
        /// Removes a passage from the locality.
        /// </summary>
        /// <param name="p">The passage to be removed</param>
        public void Unregister(Passage p)
        {
            Assert(Passages.Contains(p), "Unregistered passage");
            _passages.Remove(p.Name.Indexed);
        }

        /// <summary>
        /// Displays the locality in game world.
        /// </summary>
        protected void Appear()
        {
            foreach (Vector2 point in Area.GetPoints())
            {
                if (World.Map[point] == null)
                    World.Map[point] = new Tile(DefaultTerrain);
                else throw new InvalidOperationException("Tile must be empty");
            }
        }

        /// <summary>
        /// Erases the locality from game world.
        /// </summary>
        protected void Disappear()
        {
            // Delete objects.
            foreach (Item o in Objects)
                World.Remove(o);

            // Delete passages.
            foreach (Passage p in Passages)
                World.Remove(p);

            // Delete character.
            foreach (Character e in Characters)
                World.Remove(e);

            // Delete locality from the map.
            foreach (Vector2 p in _area.GetPoints())
                World.Map[p] = null;
        }

        /// <summary>
        /// Sends a message to all game objects in the locality.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        protected void MessageObjects(GameMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            foreach (Item o in Objects)
            {
                if (o != message.Sender)
                    o.TakeMessage(message);
            }
        }

        private void MessageEntities(GameMessage message)
        {
            foreach (Character e in Characters)
            {
                if (e != message.Sender)
                    e.TakeMessage(message);
            }
        }

        /// <summary>
        /// Handles the GameReloaded message.
        /// </summary>
        private void OnGameReloaded()
        {
            _passageLoops = new Dictionary<Passage, int>();
            _playerInHere = IsItHere(World.Player);
            UpdateLoop();
        }

        /// <summary>
        /// Handles the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message</param>
        private void OnCharacterCameToLocality(CharacterCameToLocality message)
        {
            if (message.Locality == this)
                Register(message.Character);

            if (message.Character == World.Player)
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
            foreach (Passage p in Passages)
            {
                if (p != message.Sender)
                    p.TakeMessage(message);
            }
        }


        /// <summary>
        /// Handles the LocalityLeft message.
        /// </summary>
        /// <param name="message">The message</param>
        private void OnCharacterLeftLocality(CharacterLeftLocality message)
        {
            if (message.Locality == this)
                Unregister(message.Sender as Character);

            if (message.Character == World.Player)
            {
                if (message.Locality == this)
                    _playerInHere = false;
            }
        }

        /// <summary>
        /// Handle of the stereo sound loop played when the player is inside the locality.
        /// </summary>
        [ProtoIgnore]
        private int _loopID;

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
            else if (IsAccessible(World.Player.Locality) != null)
                PlayLoop(false, true);
            else PlayLoop(false, false);
        }

        /// <summary>
        /// Stores the identifiers of location audio loops played in passages.
        /// </summary>
        [ProtoIgnore]
        private Dictionary<Passage, int> _passageLoops = new Dictionary<Passage, int>();

        private bool _reloaded;
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

            // Player is in the locality
            if (playerInHere)
            {
                _loopID = World.Sound.Play(World.Sound.GetSoundStream(_loop), null, true, PositionType.None, Vector3.Zero, false, _defaultVolume);
                return;
            }

            // Player is in a neighbour accessible locality and there's a passage between the player and this locality.
            if (accessible)
            {
                // Find passages leading to player.
                IEnumerable<Passage> exitsToPlayer = GetExitsTo(World.Player.Locality);
                foreach (Passage p in exitsToPlayer)
                {
                    Vector2 position = default;
                    Vector2 player = World.Player.Area.Center;

                    // Player stands in the passage
                    if (p.Area.Intersects(player))
                    {
                        Locality other = p.AnotherLocality(World.Player.Locality);
                        position = other.Area.GetClosestPoint(player);
                    }
                    else
                    {
                        // Is the player standing in opposit to the passage?
                        Vector2? tmp = World.Player.Area.FindOppositePoint(p.Area);
                        if (tmp.HasValue)
                            position = (Vector2)tmp;
                        else position = p.Area.GetClosestPoint(player);
                    }

                    // Make it quieter if the player is in a inadjecting locality behind a closed door.
                    Locality between = World.Player.Locality.GetLocalitiesBehindDoor().FirstOrDefault(a => a.IsBehindDoor(this));
                    bool doubleAttenuation = (between != null && World.Player.Locality.GetApertures(between).IsNullOrEmpty());
                    float volume = p.State == PassageState.Closed ? _defaultVolume : OverDoorVolume;
                    if (doubleAttenuation)
                        volume *= .01f;

                    _passageLoops[p] = World.Sound.Play(World.Sound.GetSoundStream(_loop), null, true, PositionType.Absolute, position.AsOpenALVector(), true, volume);

                    if (p.State == PassageState.Closed)
                    {
                        (float gain, float gainHF) lowpass = doubleAttenuation ? World.Sound.OverWallLowpass : World.Sound.OverDoorLowpass;
                        World.Sound.ApplyLowpass(_passageLoops[p], lowpass);
                    }
                    else
                        World.Sound.FadeSource(_passageLoops[p], FadingType.In, .00005f, _defaultVolume, false);
                }
                return;
            }

            // Player is in a distant locality or behind a wall without doors.
            _loopID = World.Sound.Play(World.Sound.GetSoundStream(_loop), null, true, PositionType.Absolute, _area.Center.AsOpenALVector(), true, OverWallVolume);
            World.Sound.ApplyLowpass(_loopID, World.Sound.OverDoorLowpass);
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
        /// <param name="fadeOut">Specifies if the loop is faded out</param>
        private void StopBackground(bool fadeOut = false)
        {
            if (fadeOut)
                World.Sound.FadeSource(_loopID, FadingType.Out, .00001f, 0);
            else World.Sound.FadeSource(_loopID, FadingType.Out, .001f, 0);

            if (!_passageLoops.IsNullOrEmpty())
            {
                foreach (int id in _passageLoops.Values)
                {
                    if (fadeOut)
                        World.Sound.FadeSource(id, FadingType.Out, .00001f, 0);
                    else World.Sound.FadeSource(id, FadingType.Out, .0001f, 0);
                }
            }

            _passageLoops = new Dictionary<Passage, int>();
        }
    }
}