using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Luky;

using Game.Terrain;

namespace Game.Entities
{
    public class DumpObject:GameObject
    {
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


        private void OnUse(Message message)
        {
            if (!string.IsNullOrEmpty(_sounds.action))
                World.Sound.Play(_sounds.action, null, false, PositionType.Absolute, Area.Center);

        }


        public override void Start()
        {
            base.Start();

            RegisterMessageHandlers(
    new Dictionary<Type, Action<Message>>()
    {
        [typeof(CollisionMessage)] =(m)=> OnCollision((CollisionMessage)m),
        [typeof(InteractionStartMessage)] = (m)=> OnUse((InteractionStartMessage)m)
    });

			if (!string.IsNullOrEmpty(_sounds.loop))
				_loopSoundId = World.Sound.Play(_sounds.loop, null, true, PositionType.Absolute, Area.Center, true);

		}


        protected void OnCollision(Message message)
        {
            if (!string.IsNullOrEmpty(_sounds.collision))
                World.Sound.Play(_sounds.collision, null, false, PositionType.Absolute, Area.Center);
        }

    }
}
