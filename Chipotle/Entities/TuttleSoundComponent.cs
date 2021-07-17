using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using Luky;

using Game.Terrain;

namespace Game.Entities
{
    public class TuttleSoundComponent : SoundComponent
    {





        public override void Start()
        {
            base.Start();

            RegisterMessages(
            new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(EntityMoved)] = (m) => OnEntityMoved((EntityMoved)m),
            }
            );

        }






        private void OnEntityMoved(EntityMoved message)
        {
            PlayTerrain(message.Target);
        }

        private void PlayTerrain(Tile tile)
        {
            string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
            _sound.Play(stream: _sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Absolute, tile.Position.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
        }
    }
}
