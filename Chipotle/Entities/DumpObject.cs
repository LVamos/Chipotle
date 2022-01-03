using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Base class for all simple game objects
    /// </summary>
    [Serializable]
    public class DumpObject : GameObject
    {
        /// <summary>
        /// Specifies if the object works as a decorator.
        /// </summary>
        /// <remarks>When it's true the object isn't reported in SaySurroundingObjects command.</remarks>
        public bool Decorative;

        /// <summary>
        /// Action sound effect handle. It expires after the sound is played.
        /// </summary>
        protected int _actionSoundID;

        /// <summary>
        /// Cutscene that should be played when the object is used by an entity
        /// </summary>
        protected string _cutscene;

        /// <summary>
        /// Background sound effect handle. It expires after the sound is stopped.
        /// </summary>
        protected int _loopSoundId;

        /// <summary>
        /// Names of sound effect files used by the object
        /// </summary>
        protected (string collision, string action, string loop) _sounds = ("MovCrashDefault", null, null);

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
        /// <remarks>
        /// The type parameter allows assigning objects with some special behavior to proper classes.
        /// </remarks>
        public DumpObject(Name name, Plane area, string type, bool decorative, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false, bool audibleOverWalls=true) : base(name, type, area)
        {
            Decorative = decorative;
            _usableOnce = usableOnce;
            _sounds = (collisionSound ?? _sounds.collision, actionSound ?? _sounds.action, loopSound ?? _sounds.loop); // Modify sounds of the object.
            _cutscene = cutscene;
            _audibleOverWalls = audibleOverWalls;
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

            RegisterMessages(
    new Dictionary<Type, Action<GameMessage>>()
    {
        [typeof(LocalityEntered)] = (message) => OnLocalityEntered((LocalityEntered)message),
        [typeof(DoorManipulated)] = (message) => OnDoorManipulated((DoorManipulated)message),
        [typeof(EntityMoved)] = (message) => OnEntityMoved((EntityMoved)message),
        [typeof(StartObjectNavigation )] = (message) => OnStartObjectNavigation((StartObjectNavigation )message),
        [typeof(StopObjectNavigation)] = (message) => OnStopObjectNavigation((StopObjectNavigation)message),
        [typeof(GameReloaded)] = (message) => OnGameReloaded(),
        [typeof(ObjectsCollided)] = (m) => OnCollision((ObjectsCollided)m),
        [typeof(UseObject)] = (m) => OnUseObject((UseObject)m)
    });

            // Play loop sound if any and if the player can hear it.
            Entity player = World.Player;
            if(IsInSameLocality(player) || Locality.GetApertures(player.Locality).Any())
            PlayLoop();
        }

        /// <summary>
        /// Handles the LocalityEntered message.
        /// </summary>
        /// <param name="message">Source of the message</param>
        private void OnLocalityEntered(LocalityEntered message)
        {
            if (message.Entity != World.Player)
                return;

            bool neighbour = message.Locality.IsNeighbour(Locality);
            bool accessible = message.Locality.IsAccessible(Locality);
            bool apertures = message.Locality.GetApertures(Locality).Any();

            if (
                !neighbour
                || (neighbour && !accessible)
                )
                StopLoop();

            else if (neighbour)
                PlayLoop(!apertures);
        }

        /// <summary>
        /// Handles the DoorManipulated message.
        /// </summary>
        /// <param name="message">The message</param>
        protected void OnDoorManipulated(DoorManipulated message)
        {
            bool open = message.Sender.State == PassageState.Open;
                Locality other = message.Sender.AnotherLocality(Locality);

            if (!other.IsItHere(World.Player))
                return;

            if (open)
            {
                PlayLoop();
                return;
            }

            // Player is in another locality and can't hear the object.
            if (!open && !_audibleOverWalls && Locality.GetApertures(other).IsNullOrEmpty())
            {
                StopLoop();
                return;
            }

            // The door is closed but the player can hear attenuated sound of the object.
            if (!open && _audibleOverWalls)
                PlayLoop(true);
        }

        /// <summary>
        /// Stops sound loop of this object, if any.
        /// </summary>
        private void StopLoop()
        {
                World.Sound.Stop(_loopSoundId);
            _loopSoundId = 0;
        }

        /// <summary>
        /// Determines if the object should be heart over walls and closed doors in other localities.
        /// </summary>
        protected readonly bool _audibleOverWalls;

        /// <summary>
        /// Returns pooint that belongs to this object and is tho most close to tthe player.
        /// </summary>
        protected Vector2 GetClosestPointToPlayer()
            => _area.GetClosestPointTo(World.Player.Area.Center);

        /// <summary>
        /// Processes the EntityMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnEntityMoved(EntityMoved message)
        {
            if (!_navigating || message.Sender != World.Player)
                return;

            World.Sound.SetSourcePosition(_navigationSoundID, GetClosestPointToPlayer().AsOpenALVector());
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
            _navigating = false;
            World.Player.ReceiveMessage(new ObjectNavigationStopped(this));
        }


        /// <summary>
        /// Name of the sound used for navigation
        /// </summary>
        protected const string _navigationSound = "SonarLoop";

        /// <summary>
        /// Name of sound played when the player passes by.
        /// </summary>
        protected const string _passBySound="ObjectPassBy";
        /// <summary>
        /// Processes the StartNavigation message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnStartObjectNavigation(StartObjectNavigation  message)
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
            _navigationSoundID = World.Sound.Play(_navigationSound, null, true, PositionType.Absolute, GetClosestPointToPlayer(), true);
            _navigating = true;
        }

        /// <summary>
        /// Handle of a soound used for navigation
        /// </summary>
        protected int _navigationSoundID;

        /// <summary>
        /// Indicates if the sound navigation is enabled.
        /// </summary>
        protected bool _navigating;
        
        /// <summary>
        /// Enables or disables sound attenuation.
        /// </summary>
        protected bool _attenuationEnabled;

        /// <summary>
        /// Default volume for the sound loop if there's any.
        /// </summary>
        protected float _defaultLoopVolume = 1;

        /// <summary>
        /// Destroys the object.
        /// </summary>
        protected override void Destroy()
        {
            base.Destroy();

            if (_loopSoundId != 0)
                World.Sound.Stop(_loopSoundId);
        }

        /// <summary>
        /// Processes the Collision message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnCollision(GameMessage message)
        {
            if (!string.IsNullOrEmpty(_sounds.collision))
                World.Sound.Play(_sounds.collision, null, false, PositionType.Absolute, Area.Center);
        }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnUseObject(UseObject message)
        {
            if ((_usableOnce && Used) || (string.IsNullOrEmpty(_sounds.action) && string.IsNullOrEmpty(_cutscene)))
                return;

            World.Sound.GetDynamicInfo(_actionSoundID, out SoundState state, out int _);
            if (state == SoundState.Playing)
                return;

            if (!string.IsNullOrEmpty(_sounds.action))
                _actionSoundID = World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_sounds.action), null, false, PositionType.Absolute, message.Position.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
            else
                World.PlayCutscene(this, _cutscene);

            UsedOnce = !Used;

            if (!Used)
                World.SaveGame();

            Used = true;
        }

        /// <summary>
        /// Handles the game reloaded message.
        /// </summary>
        private void OnGameReloaded()
        {
            if (_loopSoundId != 0)
                PlayLoop();
            else _loopSoundId = 0;
        }

        /// <summary>
        /// Plays the sound loop of this object if there's any.
        /// </summary>
        /// <param name="attenuated">Determines if the sound of the object should be played over a wall or other obstacles.</param>
        private void PlayLoop(bool attenuated=false)
        {
            if (string.IsNullOrEmpty(_sounds.loop))
                return;

            // Start the loop if not playing.
            if(_loopSoundId==0)
                _loopSoundId = World.Sound.Play(_sounds.loop, null, true, PositionType.Absolute, Area.Center, true, _defaultLoopVolume);

            // Start the sound attenuation if needed.
            if (attenuated)
            {
                World.Sound.ApplyLowpass(_loopSoundId, _lowpass);
                World.Sound.SetVolume(_loopSoundId, _defaultLoopVolume /2);
            }
            else
            {
                if (_attenuationEnabled && !attenuated)
                {
                    World.Sound.CancelLowpass(_loopSoundId);
                    World.Sound.SetVolume(_loopSoundId, _defaultLoopVolume);
                }
            }
            _attenuationEnabled = attenuated;
        }

        /// <summary>
        /// Lowpass setting for sound obstruction of the sound loop.
        /// </summary>
        protected readonly (float gain, float gainHF) _lowpass = (1, .5f);

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
        protected int GetDistanceFromPlayer()
            => World.GetDistance(GetClosestPointToPlayer(), World.Player.Area.Center);

        /// <summary>
        /// Watches distance of tthe player and turns navigation off if he approaches the object.
        /// </summary>
        private void WatchNavigation()
        {
            if (!_navigating)
                return;

            if(GetDistanceFromPlayer() <= 1 | _area.GetLocality() != World.Player.Area.GetLocality())
                StopNavigation();
        }

    }
}