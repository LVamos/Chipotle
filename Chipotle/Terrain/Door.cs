using Game.Entities;
using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;

using Luky;

using OpenTK;

using ProtoBuf;

using System.Collections.Generic;
using System.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a door between two localities.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(HallDoor))]
    [ProtoInclude(101, typeof(MariottisDoor))]
    [ProtoInclude(102, typeof(SlidingDoor))]
    [ProtoInclude(103, typeof(VanillaCrunchGarageDoor))]
    public class Door : Passage
    {
        /// <summary>
        /// Describes type of a door.
        /// </summary>
        public enum DoorType
        {
            Door,
            Gate
        }

        /// <summary>
        /// Describes type of the door.
        /// </summary>
        public DoorType Type { get; protected set; }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            WatchTimers();
        }

        /// <summary>
        /// Watches and manages timers for door manipulation.
        /// </summary>
        private void WatchTimers()
        {
            if (_manipulationTimer < _manipulationTimeLimit)
                _manipulationTimer += World.DeltaTime;
            if (_pinchTimer < _pinchTimeLimit)
                _pinchTimer += World.DeltaTime;
        }

        /// <summary>
        /// Conts time from last opening / closing.
        /// </summary>
        protected int _manipulationTimer = _manipulationTimeLimit;

        /// <summary>
        /// Conts time from last attempt to pinch an object or entity in the door.
        /// </summary>
        protected int _pinchTimer = _pinchTimeLimit;

        /// <summary>
        /// specifies if the door can be opened by an NPC.
        /// </summary>
        protected bool _openable;

        /// <summary>
        /// Sound of the door being opened
        /// </summary>
        protected string _openingSound = "snd23";

        /// <summary>
        /// Sound of the door being closed
        /// </summary>
        protected string _closingSound = "snd24";

        /// <summary>
        /// Sound played when an NPC bumps to the door.
        /// </summary>
        protected string _hitSound = "KitchenDoorCrash";

        /// <summary>
        /// Sound of the door being opened
        /// </summary>
        protected string _lockedSound;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner name for the door</param>
        /// <param name="closed">Specifies whether the door should be implicitly closed or open</param>
        /// <param name="area">Location of the door</param>
        /// <param name="localities">Two localities connected by the door</param>
        /// <param name="openable">Specifies whether the door can be opened by an NPC.</param>
        public Door(Name name, PassageState state, Rectangle area, IEnumerable<string> localities, bool openable = true, DoorType type = DoorType.Door) : base(name, area, localities)
        {
            State = state;
            _openable = openable;
            Type = type;
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case DoorHit dh: OnDoorHit(dh); break;
                case UseDoor m: OnUseDoor(m); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        ///  Handles the DoorHit message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected virtual void OnDoorHit(DoorHit message)
        {
            World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_hitSound), role: null, looping: false, PositionType.Absolute, message.Point.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);

        }

        /// <summary>
        /// Closes the door if possible
        /// </summary>
        protected void Close(object sender, Vector2 point)
        {
            if (Area.Walkable())
            {
                State = PassageState.Closed;

                AnnounceManipulation();
                Play(_closingSound, sender as Character, point);
            }
        }

        private void AnnounceManipulation()
        {
            DoorManipulated message = new DoorManipulated(this);
            IEnumerable<Locality> accessibles = Localities.First().GetAccessibleLocalities().Concat(Localities.Last().GetAccessibleLocalities()).Distinct();
            foreach (Locality l in accessibles)
                l.TakeMessage(message);
        }

        /// <summary>
        /// Plays the specified sound.
        /// </summary>
        /// <param name="sound">Name of the sound to be played</param>
        /// <param name="position"></param>
        /// <param name="obstacle">Describes type of obstacle between the entity and the player if any.</param>
        protected void Play(string sound, Character entity, Vector2 point)
        {
            // Set attenuation parameters
            ObstacleType obstacle = entity != World.Player ? World.DetectAcousticObstacles(entity.Area) : ObstacleType.None;

            if (obstacle == ObstacleType.Far)
            {
                Locality l = World.Player.Locality;
                if (Localities.First().IsBehindDoor(l) || Localities.Last().IsBehindDoor(l)) // Can be heart from the adjecting locality
                    obstacle = ObstacleType.Wall;
                else return; // Too far and inaudible
            }

            // Set attenuation parameters
            bool attenuate = obstacle != ObstacleType.None && obstacle != ObstacleType.IndirectPath; ;
            (float gain, float gainHF) lowpass = default;
            float volume = _defaultVolume;

            switch (obstacle)
            {
                case ObstacleType.Wall: lowpass = World.Sound.OverWallLowpass; volume = World.Sound.GetOverWallVolume(_defaultVolume); break;
                case ObstacleType.Door: lowpass = World.Sound.OverDoorLowpass; volume = World.Sound.GetOverDoorVolume(_defaultVolume); break;
                case ObstacleType.Object: lowpass = World.Sound.OverObjectLowpass; volume = World.Sound.GetOverObjectVolume(_defaultVolume); break;
            }

            // Play the sound
            int id = World.Sound.Play(stream: World.Sound.GetRandomSoundStream(sound), role: null, looping: false, PositionType.Absolute, point.AsOpenALVector(), true, volume);

            if (attenuate)
                World.Sound.ApplyLowpass(id, lowpass);
        }

        /// <summary>
        /// Enumerates objects and entities stand ing near the door.
        /// </summary>
        /// <returns>Enumeration of objects and entities</returns>
        protected IEnumerable<GameObject> GetObstacles()
        {
            Rectangle surroundings = Area; // Just copied
            surroundings.Extend();
            return surroundings.GetEntities().Union(_area.GetObjects());
        }

        /// <summary>
        /// A time interval in milliseconds between opening and closing.
        /// </summary>
        protected const int _manipulationTimeLimit = 800;

        /// <summary>
        /// A time interval in milliseconds between pinching an object or entity in the door.
        /// </summary>
        protected const int _pinchTimeLimit = 2500;

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnUseDoor(UseDoor message)
        {
            if (!_openable)
                return;

            if (State == PassageState.Closed && _manipulationTimer >= _manipulationTimeLimit)
            {
                _manipulationTimer = 0;
                Open(message.Sender, message.ManipulationPoint);
                return;
            }

            // The door is open. Prevent closing it if player is standing in it.
            if (_area.Intersects(World.Player.Area.Center))
            {
                if (_manipulationTimer >= _manipulationTimeLimit)
                {
                    _manipulationTimer = 0;
                    World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_hitSound), role: null, looping: false, PositionType.Absolute, message.ManipulationPoint.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
                }
                return;
            }

            // The door is open. If there are some objects or entities blocking the door then inform them that they were slammed by the door and let the door open.
            IEnumerable<GameObject> obstacles = GetObstacles().Where(o => o != World.Player);
            if (obstacles.IsNullOrEmpty()) // No obstacles, close the door.
            {
                if (_manipulationTimer >= _manipulationTimeLimit)
                {
                    _manipulationTimer = 0;
                    Close(message.Sender, message.ManipulationPoint);
                }
                return;
            }

            // The door is blocked by some objects or entities. Pinch them if the time limit has expired.
            if (_pinchTimer < _pinchTimeLimit)
                return;

            // Play a slamming sound at the nearest entity or object
            _pinchTimer = 0;

            Vector3 slamPoint = obstacles.OrderBy(o => o.Area.GetDistanceFrom(_area.Center)).First().Area.Center.AsOpenALVector();
            World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_hitSound), role: null, looping: false, PositionType.Absolute, slamPoint, true, 1f, null, _defaultVolume, 0, Playback.OpenAL);

            // Inform the obstacles without closing the door.

            foreach (GameObject o in obstacles)
            {
                PinchedInDoor pMessage = new PinchedInDoor(this, (Character)message.Sender);
                o.TakeMessage(pMessage);
            }
        }

        /// <summary>
        /// Opens the door if possible.
        /// </summary>
        /// <param name="position">
        /// The coordinates of the place on the door that an NPC is pushing on
        /// </param>
        protected virtual void Open(object sender, Vector2 point)
        {
            State = PassageState.Open;

            AnnounceManipulation();
            Play(_openingSound, sender as Character, point);
        }

        /// <summary>
        /// Returns text description of the door.
        /// </summary>
        /// <returns>text description of the door</returns>
        public override string ToString()
            => Type == DoorType.Door ? "dveře" : "vrata";
    }
}