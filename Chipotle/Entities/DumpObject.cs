using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Luky;

using Game.Terrain;

namespace Game.Entities
{
    public class DumpObject : GameObject
    {
        public bool Used { get; protected set; }
        protected int _actionSoundID { get; private set; }

        public override void Destroy()
        {
            base.Destroy();

            if (_loopSoundId != 0)
                World.Sound.Stop(_loopSoundId);
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
        /// <param name="usableOnce">Determines if the object shall be used just once</param>
        public DumpObject(Name name, Plane area, string type=null, string collisionSound=null, string actionSound=null, string loopSound=null, bool usableOnce=false) : base(name, type, area)
        {
            _usableOnce = usableOnce;
            _sounds = (collisionSound?? _sounds.collision, actionSound??_sounds.action, loopSound?? _sounds.loop); // Modify sounds of the object.

        }

        private bool _usableOnce;
        protected (string collision, string action, string loop) _sounds = ("MovCrashDefault", null, null);



        public bool UsedOnce { get; protected set; }
        protected int _loopSoundId;


        protected void OnUseObject(UseObject message)
        {
            if ((_usableOnce && Used) || string.IsNullOrEmpty(_sounds.action))
                return;

            World.Sound.GetDynamicInfo(_actionSoundID, out SoundState state, out int _);
            if (state == SoundState.Playing)
                return;

              _actionSoundID=  World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_sounds.action), null, false, PositionType.Absolute, message.Tile.Position.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);

            if (!Used)
            {
                Used = true;
                UsedOnce = true;
            }
            else UsedOnce = false;
        }


        public override void Start()
        {
            base.Start();

            RegisterMessages(
    new Dictionary<Type, Action<GameMessage>>()
    {
        [typeof(ObjectsCollided)] =(m)=> OnCollision((ObjectsCollided)m),
        [typeof(UseObject )] = (m)=> OnUseObject((UseObject )m)
    });

			if (!string.IsNullOrEmpty(_sounds.loop))
				_loopSoundId = World.Sound.Play(_sounds.loop, null, true, PositionType.Absolute, Area.Center, true);

		}


        protected void OnCollision(GameMessage message)
        {
            if (!string.IsNullOrEmpty(_sounds.collision))
                World.Sound.Play(_sounds.collision, null, false, PositionType.Absolute, Area.Center);
        }

    }
}
