using System;
using System.Collections.Generic;

using DavyKager;

using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of the Tuttle NPC
    /// </summary>
    [Serializable]
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
                [typeof(PositionChanged)] = (message) => OnPositionChanged((PositionChanged)message),
            }
            );
        }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnPositionChanged(PositionChanged message)
            => PlayTerrain(message.TargetPosition.Center, World.Map[message.TargetPosition.Center], message.Silently ? ObstacleType.Far : message.Obstacle);

        /// <summary>
        /// Plays a sound representation of a tile.
        /// </summary>
        /// <param name=positionof the tile to be announced"position"></param>
        /// <param name="tile">A tile to be announced</param>
        /// <param name="obstacle">Indicates type of an obstacle between this NPC and the player</param>
        private void PlayTerrain(Vector2 position, Tile tile, ObstacleType obstacle)
        {
            if (obstacle == ObstacleType.Far)
                return; // Too far and inaudible

            // Set attenuation parameters
            bool attenuate = obstacle != ObstacleType.None && obstacle != ObstacleType.IndirectPath; ;
            (float gain, float gainHF) lowpass = default;
            float volume = _defaultVolume;

            switch (obstacle)
            {
                case ObstacleType.Wall: lowpass = World.Sound.OverWallLowpass; volume = World.Sound.GetOverWallVolume(_defaultVolume); break;
                case ObstacleType.Door: lowpass = World.Sound.OverDoorLowpass; volume = World.Sound.GetOverDoorVolume(_defaultVolume); break;
                case ObstacleType.Object: lowpass = World.Sound.OverObjectLowpass; volume = World.Sound.GetOverObjectVolume(_defaultVolume); break;
            }

            // Play the sound
            string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
            int id = _sound.Play(stream: _sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Absolute, position.AsOpenALVector(), true, volume);

            if (attenuate)
                World.Sound.ApplyLowpass(id, lowpass);
        }
    }
}