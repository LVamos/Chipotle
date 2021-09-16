using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Base class for all simple game objects
    /// </summary>
    public class DumpObject : GameObject
    {
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
        private bool _usableOnce;

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
        public DumpObject(Name name, Plane area, string type = null, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false) : base(name, type, area)
        {
            _usableOnce = usableOnce;
            _sounds = (collisionSound ?? _sounds.collision, actionSound ?? _sounds.action, loopSound ?? _sounds.loop); // Modify sounds of the object.
            _cutscene = cutscene;
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
        [typeof(ObjectsCollided)] = (m) => OnCollision((ObjectsCollided)m),
        [typeof(UseObject)] = (m) => OnUseObject((UseObject)m)
    });

            if (!string.IsNullOrEmpty(_sounds.loop))
                _loopSoundId = World.Sound.Play(_sounds.loop, null, true, PositionType.Absolute, Area.Center, true);
        }

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
                _actionSoundID = World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_sounds.action), null, false, PositionType.Absolute, message.Tile.Position.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
            else
                World.PlayCutscene(this, _cutscene);

            if (!Used)
            {
                Used = true;
                UsedOnce = true;
            }
            else
                UsedOnce = false;
        }
    }
}