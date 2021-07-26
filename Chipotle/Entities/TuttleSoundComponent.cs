using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;

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






        private void OnEntityMoved(EntityMoved message) => PlayTerrain(message.Target);

        private void PlayTerrain(Tile tile)
        {
            string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
            _sound.Play(stream: _sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Absolute, tile.Position.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
        }
    }
}
