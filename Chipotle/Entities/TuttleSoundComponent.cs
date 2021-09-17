using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of the Tuttle NPC
    /// </summary>
    public class TuttleSoundComponent : SoundComponent
    {





        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
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






        /// <summary>
        /// Processes the  message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnEntityMoved(EntityMoved message) 
            => PlayTerrain(message.Target);

        /// <summary>
        /// Plays a sound representation of a tile.
        /// </summary>
        /// <param name="tile">A tile to be announced</param>
        private void PlayTerrain(Tile tile)
        {
            string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
            _sound.Play(stream: _sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Absolute, tile.Position.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
        }
    }
}
