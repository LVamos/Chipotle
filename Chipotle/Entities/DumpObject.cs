using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;

namespace Game.Entities
{
    public class DumpObject : GameObject
    {
        public bool Used { get; protected set; }
        protected int _actionSoundID;

        protected override void Destroy()
        {
            base.Destroy();

            if (_loopSoundId != 0)
            {
                World.Sound.Stop(_loopSoundId);
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Location of the object</param>
        /// <param name="type">Type of the object</param>
        /// <param name="collisionSound">Sound that should be played when an entity bumps to the object</param>
        /// <param name="actionSound">Sound that should be played when an entity uses the object</param>
        /// <param name="loopSound">Sound that should be played in loop</param>
        /// <param name="cutscene">Cutscene that should be played</param>
        /// <param name="usableOnce">Determines if the object shall be used just once</param>
        public DumpObject(Name name, Plane area, string type = null, string collisionSound = null, string actionSound = null, string loopSound = null, string cutscene = null, bool usableOnce = false) : base(name, type, area)
        {
            _usableOnce = usableOnce;
            _sounds = (collisionSound ?? _sounds.collision, actionSound ?? _sounds.action, loopSound ?? _sounds.loop); // Modify sounds of the object.
            _cutscene = cutscene;
        }

        private bool _usableOnce;
        protected (string collision, string action, string loop) _sounds = ("MovCrashDefault", null, null);
        protected string _cutscene;

        public bool UsedOnce { get; protected set; }
        protected int _loopSoundId;


        protected virtual void OnUseObject(UseObject message)
        {
            if ((_usableOnce && Used) || (string.IsNullOrEmpty(_sounds.action) && string.IsNullOrEmpty(_cutscene)))
            {
                return;
            }

            World.Sound.GetDynamicInfo(_actionSoundID, out SoundState state, out int _);
            if (state == SoundState.Playing)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_sounds.action))
            {
                _actionSoundID = World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_sounds.action), null, false, PositionType.Absolute, message.Tile.Position.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
            }
            else
            {
                World.PlayCutscene(this, _cutscene);
            }

            if (!Used)
            {
                Used = true;
                UsedOnce = true;
            }
            else
            {
                UsedOnce = false;
            }
        }


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
            {
                _loopSoundId = World.Sound.Play(_sounds.loop, null, true, PositionType.Absolute, Area.Center, true);
            }
        }


        protected void OnCollision(GameMessage message)
        {
            if (!string.IsNullOrEmpty(_sounds.collision))
            {
                World.Sound.Play(_sounds.collision, null, false, PositionType.Absolute, Area.Center);
            }
        }

    }
}
