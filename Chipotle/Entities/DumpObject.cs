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
        public override void Destroy()
        {
            base.Destroy();

            if (_loopSoundId != 0)
                World.Sound.Stop(_loopSoundId);
        }

        public DumpObject(Name name, Plane area, string type=null, string collisionSound=null, string actionSound=null, string loopSound=null) : base(name, type, area)
        {
            _sounds = (collisionSound?? _sounds.collision, actionSound??_sounds.action, loopSound?? _sounds.loop); // Modify sounds of the object.

        }

        protected (string collision, string action, string loop) _sounds = ("MovCrashDefault", null, null);


        protected int _loopSoundId;


        private void OnUseObject(UseObject message)
        {
            if (string.IsNullOrEmpty(_sounds.action))
                return;

                World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_sounds.action), null, false, PositionType.Absolute, message.Tile.Position.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
            Used = true;
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
