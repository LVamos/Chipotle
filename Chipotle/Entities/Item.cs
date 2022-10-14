using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Entities
{
    /// <summary>
    /// Base class for all simple game objects
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(CarsonsBench))]
    [ProtoInclude(101, typeof(CarsonsGrill))]
    [ProtoInclude(102, typeof(Corpse))]
    [ProtoInclude(103, typeof(ChipotlesCar))]
    [ProtoInclude(104, typeof(ChristinesBell))]
    [ProtoInclude(105, typeof(IcecreamMachine))]
    [ProtoInclude(106, typeof(KeyHanger))]
    [ProtoInclude(107, typeof(KillersCar))]
    [ProtoInclude(108, typeof(PoolsideBin))]
    [ProtoInclude(109, typeof(PubBench))]
    [ProtoInclude(110, typeof(SweeneysBell))]
    [ProtoInclude(111, typeof(VanillaCrunchCar))]
    public class Item : GameObject
    {
        /// <summary>
        /// React on placing on the ground.
        /// </summary>
        protected void Placed() => Play(_sounds.placing);

        /// <summary>
        /// Backing field for Localities property.
        /// </summary>
        protected HashSet<string> _localities = new HashSet<string>();

        /// <summary>
        /// Localities intersecting with this object.
        /// </summary>
        [ProtoIgnore]
        public IEnumerable<Locality> Localities
        {
            get
            {
                return
                    from name in _localities
                    select World.GetLocality(name);
            }
        }

        /// <summary>
        /// Finds all localities the object or the NPC intersects with and saves their names into _localities.
        /// </summary>
        protected void FindLocalities()
        {
            if (_area == null)
                return;

            _localities =
            (from l in World.GetLocalities(_area)
             select l.Name.Indexed)
            .ToHashSet<string>();
        }


        /// <summary>
        /// Sets value of the Area property.
        /// </summary>
        /// <param name="value">A value assigned to the property</param>
        protected override void SetArea(Rectangle value)
        {
            base.SetArea(value);

            // Find out which localities the object now intersects, then sends the appropriate messages to the concerning localities.
            Locality[] backup = Localities.ToArray<Locality>();
            FindLocalities();

            // Inform concerning localities that the object disappeared.
            foreach (Locality l in backup)
            {
                if (!Localities.Contains(l))
                    l.TakeMessage(new ObjectDisappearedFromLocality(this, this, l));
            }

            // Inform concerning localities that the object appeared.
            foreach (Locality l in Localities)
            {
                if (!backup.Contains(l))
                    l.TakeMessage(new ObjectAppearedInLocality(this, this, l));
            }
        }

        /// <summary>
        /// Specifies if the object is held by an entity.
        /// </summary>
        public Character HeldBy { get; protected set; }

        /// <summary>
        /// Checks if the object can be picked up off the ground in the moment.
        /// </summary>
        /// <returns>True if the object can be picked up off the ground.</returns>
        protected virtual bool CanBePicked() => _pickable && HeldBy == null;

        /// <summary>
        /// Indicates if the object stops playing its action sound when the player moves or turns.
        /// </summary>
        protected readonly bool _stopWhenPlayerMoves;

        /// <summary>
        /// Specifies if the object works as a decorator.
        /// </summary>
        /// <remarks>When it's true the object isn't reported in SaySurroundingObjects command.</remarks>
        public bool Decorative;

        /// <summary>
        /// Action sound effect handle. It expires after the sound is played.
        /// </summary>
        [ProtoIgnore]
        protected int _actionSoundID;

        /// <summary>
        /// Cutscene that should be played when the object is used by an entity
        /// </summary>
        protected string _cutscene;

        /// <summary>
        /// Background sound effect handle. It expires after the sound is stopped.
        /// </summary>
        [ProtoIgnore]
        protected int _loopSoundId;

        /// <summary>
        /// Names of sound effect files used by the object
        /// </summary>
        protected (string collision, string action, string loop, string picking, string placing) _sounds = ("MovCrashDefault", null, null, null, null);

        /// <summary>
        /// Determines if the object shall be used just once
        /// </summary>
        private readonly bool _usableOnce;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <param name="type">Type of the object</param>
        /// <param name="collisionSound">
        /// Sound that should be played when an entity bumps to the object
        /// </param>
        /// <param name="actionSound">Sound that should be played when an entity uses the object</param>
        /// <param name="loopSound">Sound that should be played in loop on background</param>
        /// <param name="cutscene">
        /// Cutscene that should be played when the object is used by an entity
        /// </param>
        /// <param name="usableOnce">Determines if the object shall be used just once</param>
        /// <param name="audibleOverWalls"></param>
        /// <param name="decorative">Determines if the object is just a decoration</param>
        /// <param name="pickable">Determines if the object can be picked by a character</param>
        /// <param name="pickingSound">A sound played when the object is picked by a character</param>
        /// <param name="placingSound">A sound played when the object is placed by a character</param>
        /// <param name="quickActionsAllowed">Specifies if the item can be used in rapid succession.</param>
        /// <param name="stopWhenPlayerMoves">Specifies if an ongoing action sound stops when the player moves.</param>
        /// <param name="volume">Specifies individual volume for sounds made by the object.</param>
        /// <remarks>
        /// The type parameter allows assigning objects with some special behavior to proper classes.
        /// </remarks>
        public Item(Name name, Rectangle area, string type, bool decorative, bool pickable, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls = true, float volume = 1, bool stopWhenPlayerMoves = false, bool quickActionsAllowed = false, string pickingSound = null, string placingSound = null) : base(name, type, area)
        {
            Area = area;

            Decorative = decorative;
            _pickable = pickable;
            _usableOnce = usableOnce;
            _sounds = (collisionSound ?? _sounds.collision, actionSound ?? _sounds.action, loopSound ?? _sounds.loop, pickingSound ?? _sounds.picking, placingSound ?? _sounds.placing); // Modify sounds of the object.
            _cutscene = cutscene;
            _audibleOverWalls = audibleOverWalls;
            _defaultVolume = volume;
            _stopWhenPlayerMoves = stopWhenPlayerMoves;
            _quickActionsAllowed = quickActionsAllowed;
        }

        /// <summary>
        /// Indicates if the object was used by an entity.
        /// </summary>
        public bool Used { get; protected set; }

        /// <summary>
        /// Determines if the object shall be used just once
        /// </summary>
        public bool UsedOnce { get; protected set; }

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Add the object to intersecting localities
            foreach (Locality l in Localities)
                l.TakeMessage(new ObjectAppearedInLocality(this, this, l));

            // Play loop sound if any and if the player can hear it.
            UpdateLoop();
        }

        /// <summary>
        /// Processes the Collision message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnOrientationChanged(OrientationChanged message)
=> WatchPlayersMovement();
        protected virtual void OnLocalityEntered(CharacterCameToLocality message)
            => UpdateLoop();

        /// <summary>
        /// Checks if there's a direct path from this object to the player.
        /// </summary>
        /// <returns>True if there's a direct path from this object to the player</returns>
        protected ObstacleType GetObstacles()
        {
            Vector2 player = World.Player.Area.Center;
            Vector2 me = _area.GetClosestPoint(player);
            Rectangle path = new Rectangle(me, player);

            return World.DetectObstacles(path);
        }

        /// <summary>
        /// Handles the DoorManipulated message.
        /// </summary>
        /// <param name="message">The message</param>
        protected void OnDoorManipulated(DoorManipulated message)
            => UpdateLoop();

        /// <summary>
        /// Stops sound loop of this object, if any.
        /// </summary>
        protected void StopLoop()
        {
            World.Sound.Stop(_loopSoundId);
            _loopSoundId = 0;
        }

        /// <summary>
        /// Determines if the object should be heart over walls and closed doors in other localities.
        /// </summary>
        protected readonly bool _audibleOverWalls;

        /// <summary>
        /// Processes the EntityMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnEntityMoved(CharacterMoved message)
        {
            if (message.Sender != World.Player)
                return;

            UpdateNavigatingSound();
            UpdateLoop();
            WatchPlayersMovement();
        }

        /// <summary>
        /// Stops the action sound.
        /// </summary>
        protected void WatchPlayersMovement()
        {
            if (_stopWhenPlayerMoves && _actionSoundID > 0)
                World.Sound.Stop(_actionSoundID);
        }

        /// <summary>
        /// Processes the StopNavigation message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnStopObjectNavigation(StopObjectNavigation message)
        {
            if (message.Sender != World.Player)
                throw new ArgumentException(nameof(message.Sender));

            StopNavigation();
        }

        /// <summary>
        /// Stops the sound navigation.
        /// </summary>
        protected void StopNavigation()
        {
            World.Sound.ChangeLooping(_navigationSoundID, false);
            _navigationSoundID = 0;
            _navigating = false;
            World.Player.TakeMessage(new ObjectNavigationStopped(this));
        }


        /// <summary>
        /// Name of the sound used for navigation
        /// </summary>
        protected const string _navigationSound = "SonarLoop";

        /// <summary>
        /// Name of sound played when the player passes by.
        /// </summary>
        protected const string _passBySound = "ObjectPassBy";
        /// <summary>
        /// Processes the StartNavigation message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnStartObjectNavigation(StartObjectNavigation message)
        {
            if (message.Sender != World.Player)
                throw new ArgumentException(nameof(message.Sender));

            StartNavigation();
        }

        /// <summary>
        /// Starts sound navigation.
        /// </summary>
        private void StartNavigation()
        {
            ReportPosition(true);
            _navigating = true;
            UpdateNavigatingSound();
        }

        /// <summary>
        /// Plays a navigation sound on position of the object.
        /// </summary>
        /// <param name="loop">Specifies if the navigating soudn should be played in loop</param>
        protected virtual void ReportPosition(bool loop = false)
            => _navigationSoundID = Play(_navigationSound, loop);

        /// <summary>
        /// Handle of a soound used for navigation
        /// </summary>
        protected int _navigationSoundID;

        /// <summary>
        /// Indicates if the sound navigation is enabled.
        /// </summary>
        [ProtoIgnore]
        protected bool _navigating;

        /// <summary>
        /// Enables or disables sound attenuation.
        /// </summary>
        protected bool _attenuate;
        private bool _quickActionsAllowed;

        /// <summary>
        /// Specifies if the object can be carried.
        /// </summary>
        protected bool _pickable;

        /// <summary>
        /// Destroys the object.
        /// </summary>
        protected override void Destroy()
        {
            base.Destroy();

            if (_loopSoundId != 0)
                World.Sound.Stop(_loopSoundId);

            // Inform localities that the object disappeared.
            foreach (Locality l in Localities)
                l.TakeMessage(new ObjectDisappearedFromLocality(this, this, l));
        }

        /// <summary>
        /// Processes the Collision message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnObjectsCollided(ObjectsCollided message) => Play(_sounds.collision, message.Position);

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnUseObjects(UseObjects message)
        {
            if ((_usableOnce && Used) || (string.IsNullOrEmpty(_sounds.action) && string.IsNullOrEmpty(_cutscene)))
                return;

            World.Sound.GetDynamicInfo(_actionSoundID, out SoundState state, out int sample);

            if (state != SoundState.Playing || (state == SoundState.Playing && sample > 20000 && _quickActionsAllowed))
            {
                if (!string.IsNullOrEmpty(_sounds.action))
                    _actionSoundID = Play(_sounds.action, message.ManipulationPoint);
                else
                    World.PlayCutscene(this, _cutscene);
            }

            UsedOnce = !Used;
            Used = true;
        }

        /// <summary>
        /// Handles the game reloaded message.
        /// </summary>
        private void OnGameReloaded()
            => UpdateLoop();

        /// <summary>
        /// Plays the sound loop of this object if there's any.
        /// </summary>
        /// <param name="attenuated">Determines if the sound of the object should be played over a wall or other obstacles.</param>
        protected void UpdateLoop()
        {
            if (string.IsNullOrEmpty(_sounds.loop))
                return;

            ObstacleType obstacle = World.DetectAcousticObstacles(_area);
            if (obstacle == ObstacleType.Far)
                StopLoop();
            else PlayLoop(obstacle);// != ObstacleType.Wall ? obstacle: ObstacleType.Object);
        }

        /// <summary>
        /// Plays sound loop of the object with sound attenuation.
        /// </summary>
        /// <param name="obstacle">Type of obstacle between player and this object</param>
        protected void PlayLoop(ObstacleType obstacle = ObstacleType.None)
        {
            // Start the loop if not playing.
            if (_loopSoundId == 0)
                _loopSoundId = Play(_sounds.loop, _area.Center, true);

            // Start the sound attenuation if needed.
            bool attenuate = obstacle != ObstacleType.None && obstacle != ObstacleType.IndirectPath; ;
            (float gain, float gainHF) lowpass = default;
            float volume = 0;

            switch (obstacle)
            {
                case ObstacleType.Wall: lowpass = World.Sound.OverWallLowpass; volume = OverWallVolume; break;
                case ObstacleType.Door: lowpass = World.Sound.OverDoorLowpass; volume = OverDoorVolume; break;
                case ObstacleType.Object: lowpass = World.Sound.OverObjectLowpass; volume = OverObjectVolume; break;
            }

            if (attenuate)
            {
                World.Sound.ApplyLowpass(_loopSoundId, lowpass);
                World.Sound.FadeSource(_loopSoundId, FadingType.Out, .00005f, volume, false);
            }
            else
            {
                // Turn of attenuation
                if (_attenuate && !attenuate)
                {
                    World.Sound.CancelLowpass(_loopSoundId);
                    World.Sound.FadeSource(_loopSoundId, FadingType.In, .00005f, _defaultVolume);
                }
            }

            _attenuate = attenuate;
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            WatchNavigation();
        }

        /// <summary>
        /// Returns distance from this object to the player.
        /// </summary>
        /// <returns>Distance in meters</returns>
        protected float GetDistanceFromPlayer()
            => World.GetDistance(GetClosestPointToPlayer(), World.Player.Area.Center);

        /// <summary>
        /// Updates position and attenuation of navigating sound if the navigation is in progress.
        /// </summary>
        private void UpdateNavigatingSound()
        {
            if (!_navigating)
                return;

            // To give the player the impression that the navigation sound is heard over the entire object its position is set to the coordinates of the object point closest to the player.
            World.Sound.SetSourcePosition(_navigationSoundID, GetClosestPointToPlayer().AsOpenALVector());

            // Find opposite point
            Vector2? opposite = World.Player.Area.FindOppositePoint(_area);
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
        /// Watches distance of tthe player and turns navigation off if he approaches the object.
        /// </summary>
        private void WatchNavigation()
        {
            if (!_navigating)
                return;

            float distance = GetDistanceFromPlayer();
            if (distance <= 1 | distance >= 50 || !SameLocality(World.Player))
                StopNavigation();
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case PlaceObject m: OnPlaceObject(m); break;
                case PickUpObject m: OnPickUpObject(m); break;
                case ReportPosition m: OnReportPosition(m); break;
                case OrientationChanged oc: OnOrientationChanged(oc); break;
                case CharacterCameToLocality le: OnLocalityEntered(le); break;
                case CharacterMoved em: OnEntityMoved(em); break;
                case DoorManipulated dm: OnDoorManipulated(dm); break;
                case StartObjectNavigation so: OnStartObjectNavigation(so); break;
                case StopObjectNavigation sto: OnStopObjectNavigation(sto); break;
                case GameReloaded gr: OnGameReloaded(); break;
                case ObjectsCollided oc: OnObjectsCollided(oc); break;
                case UseObjects uo: OnUseObjects(uo); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="m">Source of the message</param>
        private void OnPlaceObject(PlaceObject m)
        {
            Rectangle vacancy = FindVacancy(m.Position);
            bool success = vacancy != null;

            if (success)
            {
                HeldBy = null;
                Area = vacancy;
                Placed();
            }

            m.Sender.TakeMessage(new PlaceObjectResult(this, success));
        }

        /// <summary>
        /// Tries to find a free spot on the ground to place the object on.
        /// </summary>
        /// <param name="point">A point that should intersect with the placed object</param>
        /// <returns>An area the object could be placed on</returns>
        protected Rectangle FindVacancy(Vector2 point)
        {
            Rectangle area = new Rectangle(point);
            (float width, float height)[] testDimensions = new[]
            {
            (Dimensions.height, Dimensions.width),
            (Dimensions.width, Dimensions.height)
            };

            /*
			 * Create a test rectangle that only occupies the specified point. 
			 * Then systematically expand the rectangle in all directions until it intersects the player or its dimensions correspond to the dimensions of the object placed on the ground.
			 */
            foreach ((float height, float width) d in testDimensions)
            {
                // Extend it upward
                while (area.Walkable() && area.Height < d.height)
                    area.Extend(Direction.Up);

                // Extend it downward if necessary.
                while (area.Walkable() && area.Height < d.height)
                    area.Extend(Direction.Down);

                // Extend it to the left
                while (area.Walkable() && area.Width < d.width)
                    area.Extend(Direction.Left);

                // Extend it to the right if necessary.
                while (area.Walkable() && area.Width < d.width)
                    area.Extend(Direction.Right);

                if ((area.Height, area.Width) == d)
                    return new Rectangle(area);
            }

            return null;
        }

        /// <summary>
        /// Handles the PickUpObject message.
        /// </summary>
        /// <param name="m">The message to be handled</param>
        protected void OnPickUpObject(PickUpObject m)
        {
            PickUpObjectResult.ResultType result = CanBePicked() ? PickUpObjectResult.ResultType.Success : PickUpObjectResult.ResultType.Unpickable;

            if (result == PickUpObjectResult.ResultType.Success)
            {
                Picked();
                HeldBy = (Character)m.Sender;
                Area = null;
            }

            // Report the result.
            m.Sender.TakeMessage(new PickUpObjectResult(this, this, result));
        }

        /// <summary>
        /// Reacts on picking off the ground.
        /// </summary>
        protected virtual void Picked() => Play(_sounds.picking);

        /// <summary>
        /// Handles the ReportPosition message.
        /// </summary>
        /// <param name="m">The message to be handled</param>
        private void OnReportPosition(ReportPosition m)
=> ReportPosition();
    }
}