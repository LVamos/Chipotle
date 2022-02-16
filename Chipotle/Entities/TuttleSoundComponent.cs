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
        public TuttleSoundComponent(): base()
            => _walkingVolume = .2f;

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case PositionChanged pc:  OnPositionChanged(pc); break;
                default: base.HandleMessage(message); break;
            }
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
            float volume = _walkingVolume;

            switch (obstacle)
            {
                case ObstacleType.Wall: lowpass = World.Sound.OverWallLowpass; volume = World.Sound.GetOverWallVolume(_walkingVolume); break;
                case ObstacleType.Door: lowpass = World.Sound.OverDoorLowpass; volume = World.Sound.GetOverDoorVolume(_walkingVolume); break;
                case ObstacleType.Object: lowpass = World.Sound.OverObjectLowpass; volume = World.Sound.GetOverObjectVolume(_walkingVolume); break;
            }

            // Play the sound
            string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
            int id = _sound.Play(stream: _sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Absolute, position.AsOpenALVector(), true, volume);

            if (attenuate)
                World.Sound.ApplyLowpass(id, lowpass);
        }
    }
}