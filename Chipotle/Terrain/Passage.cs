using Game.Messaging;
using Game.Messaging.Commands;
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
    /// Represents a passage between two localities.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(Door))]
    public class Passage : MapElement
    {
        /// <summary>
        /// Returns text description of the passage.
        /// </summary>
        /// <returns>text description of the passage</returns>
        public override string ToString()
            => "průchod";

        /// <summary>
        /// Checks if the specified point lays in front or behind the passage.
        /// </summary>
        /// <returns>True if the specified point lays in front or behind the passage</returns>
        public bool IsInFrontOrBehind(Vector2 point)
        {
            //if (Name.Indexed == "p chodba w1 jídelna w1") System.Diagnostics.Debugger.Break();

            return
 IsInRelatedLocality(point)
        && (
            (IsHorizontal() && (point.X >= _area.UpperLeftCorner.X && point.X <= _area.UpperRightCorner.X))
            || (IsVertical() && (point.Y >= _area.LowerLeftCorner.Y && point.Y <= _area.UpperLeftCorner.Y))
            );
        }

        /// <summary>
        /// Chekcs if the specified point lays in one of the localities connected by the passage.
        /// </summary>
        /// <param name="point">The point to be checked</param>
        /// <returns>True if the specified point lays in one of the localities connected by the passage</returns>
        public bool IsInRelatedLocality(Vector2 point)
            => Localities.Any(l => l.Area.Intersects(point));

        /// <summary>
        /// Checks if the passage is horizontal.
        /// </summary>
        /// <returns>True if the passage is horizontal</returns>
        public bool IsHorizontal()
        {
            // Tests if both upper left corner and lower left corner lay in different localities (faster than World.GetLocality)
            return
            Localities.First().Area.Intersects(_area.UpperLeftCorner)
            ^ Localities.First().Area.Intersects(_area.LowerLeftCorner);
        }

        /// <summary>
        /// Checks if the passage is vertical.
        /// </summary>
        /// <returns>True if the passage is vertical</returns>
        public bool IsVertical()
                    => !IsHorizontal();

        /// <summary>
        /// Indicates if the door is open or closed.
        /// </summary>
        public PassageState State { get; protected set; } = PassageState.Open;

        /// <summary>
        /// Checks if the passage leads to the specified locality.
        /// </summary>
        /// <param name="l">The locality to be checked</param>
        /// <returns>True if the passage leads to the specified locality</returns>
        public bool LeadsTo(Locality l)
            => Localities.Contains(l);

        /// <summary>
        /// Localities connected by the passage
        /// </summary>
        [ProtoIgnore]
        public IEnumerable<Locality> Localities
        {
            get
            {
                if (_localities == null)
                    _localities = new string[2];

                return _localities.Select(l => World.GetLocality(l)).Where(l => l != null);
            }
        }

        /// <summary>
        /// Localities connected by the passage
        /// </summary>
        private string[] _localities = new string[2];

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the passage</param>
        /// <param name="area">Coordinates of the are occupied by the passage</param>
        /// <param name="localities">Localities connected by the passage</param>
        public Passage(Name name, Rectangle area, IEnumerable<string> localities) : base(name, area)
        {
            // Check if the passage occupies just one row or column.
            Assert(area.Height == 1 || area.Height == 2 || area.Width == 1 || area.Width == 2, "Passage must consist of two rows or two points.");

            Assert(localities != null && localities?.Count() == 2 && localities.First() != null && localities.Last() != null && localities.First() != localities.Last(), "Two different localities required");
            _localities = localities.ToArray<string>();

            // Validate passage location
            Assert(area.GetObjects().IsNullOrEmpty() && area.GetPassages().IsNullOrEmpty(), "No objects or nested passages allowed");

            Appear();
        }

        /// <summary>
        /// Creates new instance of a sliding door.
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the are occupied by the door</param>
        /// <param name="localities">Localities connected by the door</param>
        /// <returns>A new instance of the door</returns>
        public static SlidingDoor CreateSlidingDoor(Name name, Rectangle area, IEnumerable<string> localities)
=> new SlidingDoor(name, area, localities);

        /// <summary>
        /// Creates new instance of the door in the hall of the Vanilla crunch company (hala v1) locality.
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the are occupied by the door</param>
        /// <param name="localities">Localities connected by the door</param>
        /// <returns>A new instance of the door</returns>
        public static HallDoor CreateHallDoor(Name name, Rectangle area, IEnumerable<string> localities)
=> new HallDoor(name, area, localities);

        /// <summary>
        /// Creates new instance of the door between the hall of the Vanilla crunch company (hala
        /// v1) locality and the office of the Paolo Mariotti NPC's office (kancelář v1) locality.
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the are occupied by the door</param>
        /// <param name="localities">Localities connected by the door</param>
        /// <returns>A new instance of the door</returns>
        public static MariottisDoor CreateMariottisDoor(Name name, Rectangle area, IEnumerable<string> localities)
=> new MariottisDoor(name, area, localities);

        /// <summary>
        /// Creates new instance of a passage according to the given parameters.
        /// </summary>
        /// <param name="name">Inner name of the passage</param>
        /// <param name="area">Coordinates of the are occupied by the passage</param>
        /// <param name="localities">Localities connectedd by the passage</param>
        /// <param name="isDoor">Specifies if the passage is a door.</param>
        /// <param name="closed">Specifies if the passage is closed.</param>
        /// <param name="openable">Specifies if it can be opened by an NPC.</param>
        /// <param name="state">State of a door</param>
        /// <param name="type">Type of a door</param>
        /// <returns>A new instance of the passage</returns>
        public static Passage CreatePassage(Name name, Rectangle area, IEnumerable<string> localities, bool isDoor, PassageState state, bool openable, Door.DoorType type)
        {
            switch (name.Indexed)
            {
                case "duhv1": return CreateSlidingDoor(name, area, localities);
                case "dcgv1": return CreateVanillaCrunchGarageDoor(name, area, localities);
                case "dhkv1": return CreateMariottisDoor(name, area, localities);
                case "d hala w1": return CreateHallDoor(name, area, localities);
                default: return isDoor ? new Door(name, state, area, localities, openable, type) : new Passage(name, area, localities);
            }
        }

        /// <summary>
        /// Returns another side of this passage.
        /// </summary>
        /// <param name="comparedLocality">The locality to be compared</param>
        /// <returns>The other side of the passage than the specified one</returns>
        public Locality AnotherLocality(Locality comparedLocality)
            => Localities.First(l => l.Name.Indexed != comparedLocality.Name.Indexed);

        /// <summary>
        /// Displays the passage in the game world.
        /// </summary>
        protected void Appear()
        {
            Area.GetTiles().Foreach(t => t.tile.Register(World.GetLocality(t.position).DefaultTerrain));
            Localities.Foreach(l => l.Register(this));
        }

        /// <summary>
        /// Erases the passage from the game world.
        /// </summary>
        protected void Disappear()
        {
            foreach (Locality l in Localities)
                l.Unregister(this);
        }

        /// <summary>
        /// Creates new instance of the garage door in the garage of the Vanilla crunch company
        /// (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the are occupied by the door</param>
        /// <param name="localities">Localities connected by the door</param>
        /// <returns>A new instance of the door</returns>
        private static Passage CreateVanillaCrunchGarageDoor(Name name, Rectangle area, IEnumerable<string> localities)
            => new VanillaCrunchGarageDoor(name, area, localities);


        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            WatchNavigation();
        }

        /// <summary>
        /// Handle of a sound used for navigation.
        /// </summary>
        [ProtoIgnore]
        protected int _navigationSoundID;

        /// <summary>
        /// Indicates if sound navigation is in progress.
        /// </summary>
        [ProtoIgnore]
        protected bool _navigating;

        /// <summary>
        /// Starts sound navigation.
        /// </summary>
        protected void StartNavigation()
        {
            _playersLocality = World.Player.Locality;
            _navigationSoundID = World.Sound.Play(_navigationSound, null, true, PositionType.Absolute, GetClosestPointToPlayer(), true);
            _navigating = true;
            UpdateNavigatingSound();
        }

        /// <summary>
        /// Name of the sound used for navigation
        /// </summary>
        protected const string _navigationSound = "ExitLoop";

        /// <summary>
        /// Returns pooint that belongs to this object and is the most close to the player.
        /// </summary>
        protected Vector2 GetClosestPointToPlayer()
            => _area.GetClosestPoint(World.Player.Area.Center);

        /// <summary>
        /// stops ongoing sound navigation.
        /// </summary>
        protected void StopNavigation()
        {
            World.Sound.ChangeLooping(_navigationSoundID, false);
            _navigating = false;
            World.Player.TakeMessage(new ExitNavigationStopped(this));
        }

        /// <summary>
        /// Watches between this passage and the player and terminates navigation if he gets close.
        /// </summary>
        protected void WatchNavigation()
        {
            if (!_navigating)
                return;

            float distance = GetDistanceFromPlayer();
            if (distance <= 1 || distance >= 50 | _playersLocality != World.Player.Locality)
                StopNavigation();
        }

        /// <summary>
        /// stores a locality in which the player is located after navigation start.
        /// </summary>
        [ProtoIgnore]
        protected Locality _playersLocality;

        /// <summary>
        /// Indicates if the sound attenuation is enabled.
        /// </summary>
        [ProtoIgnore]
        private bool _attenuate;

        /// <summary>
        /// Returns distance from this passage to the player.
        /// </summary>
        /// <returns>Distance in meters</returns>
        protected float GetDistanceFromPlayer()
            => World.GetDistance(GetClosestPointToPlayer(), World.Player.Area.Center);

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case CharacterMoved em: OnEntityMoved(em); break;
                case StartExitNavigation sen: OnStartExitNavigation(sen); break;
                case StopExitNavigation stp: OnStopExitNavigation(stp); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Processes the EntityMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnEntityMoved(CharacterMoved message)
        {
            if (!_navigating || message.Sender != World.Player)
                return;

            UpdateNavigatingSound();
        }

        /// <summary>
        /// Updates position and attenuation of navigating sound if the navigation is in progress.
        /// </summary>
        private void UpdateNavigatingSound()
        {
            // To give the player the impression that the navigation sound is heard over the entire passage area, his position is set to the coordinates of the passage point closest to the player.
            World.Sound.SetSourcePosition(_navigationSoundID, GetClosestPointToPlayer().AsOpenALVector());

            // Get side of the are that lays in the same locality as the player.
            Rectangle side =
            (
            from d in DirectionExtension.BasicDirections
            let s = Area.GetSide(d)
            where (s.GetLocalities() == World.Player.Locality)
            select s
                ).FirstOrDefault();
            if (side == null)
                throw new InvalidOperationException(nameof(UpdateNavigatingSound));

            // Find opposite point
            Vector2? opposite = World.Player.Area.FindOppositePoint(side);
            if (!opposite.HasValue)
                return; // Sound isn't blocked, play it normally.

            // Detect potentional acoustic obstacles and set up attenuate parameters
            ObstacleType obstacle = World.DetectAcousticObstacles(new Rectangle((Vector2)opposite));
            float volume;
            (float gain, float gainHF) lowpass;
            bool attenuate = obstacle == ObstacleType.Wall || obstacle == ObstacleType.Object;
            if (obstacle == ObstacleType.Wall)
            {
                World.Sound.ApplyLowpass(_navigationSoundID, World.Sound.OverWallLowpass);
                World.Sound.SetSourceVolume(_navigationSoundID, OverWallVolume);
            }
            else if (obstacle == ObstacleType.Object)
            {
                World.Sound.ApplyLowpass(_navigationSoundID, World.Sound.OverObjectLowpass);
                World.Sound.SetSourceVolume(_navigationSoundID, OverObjectVolume);
            }
            else if (_attenuate)
            {
                // Turn off attenuation
                World.Sound.CancelLowpass(_navigationSoundID);
                World.Sound.SetSourceVolume(_navigationSoundID, _defaultVolume);
            }

            _attenuate = attenuate;
        }

        /// <summary>
        /// Processes the StopExitNavigation message.
        /// </summary>
        /// <param name="message">Source of the message</param>
        protected void OnStopExitNavigation(StopExitNavigation message)
        {
            if (message.Sender != World.Player)
                throw new ArgumentException(nameof(message.Sender));

            StopNavigation();
        }

        /// <summary>
        /// Processes the StartExitNavigation message.
        /// </summary>
        /// <param name="message">Source of the message</param>
        protected void OnStartExitNavigation(StartExitNavigation message)
        {
            if (message.Sender != World.Player)
                throw new ArgumentException(nameof(message.Sender));

            StartNavigation();
        }
    }
}