using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

using ProtoBuf;

using System;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of the Tuttle NPC
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class TuttleSoundComponent : SoundComponent
    {
        /// <summary>
        /// A sound played when Tuttle gets pinched in a door by another NPC.
        /// </summary>
        protected const string _doorPinchSound = "TuttleHitByDoor";

        /// <su mmary>
        /// Handle of sound with Tuttle's speech that can move when Tuttle is walking.
        /// </summary>
        [ProtoBuf.ProtoIgnore]
        protected int _movingSpeechHandle;

        /// <summary>
        /// Target position for current Tuttle's voice sliding.
        /// </summary>
        [ProtoBuf.ProtoIgnore]
        protected Vector3 _voiceSlideTarget;

        /// <summary>
        /// Current position for Tuttle's voice sliding.
        /// </summary>
        protected Vector3 _voiceSlidePosition;

        /// <summary>
        /// Specifies how the current position of Tuttle's voice changes in one step when voice sliding is being performed.
        /// </summary>
        [ProtoBuf.ProtoIgnore]
        protected Vector3 _voiceSlideDelta;

        /// <summary>
        /// Specifies how much steps are needed to perform the voice slide.
        /// </summary>
        protected int _voiceSlideTicks => _voiceSlideInterval / World.DeltaTime;

        /// <summary>
        /// A counter used for voice sliding.
        /// </summary>
        [ProtoBuf.ProtoIgnore]
        protected int _voiceSlideTimer = -1;

        /// <summary>
        /// Sound with reaction on collision with Chipotle.
        /// </summary>
        private const string _chipotleCollisionSound = "TuttleHitByDoor";

        /// <summary>
        /// Specifies how long it takes to slide position of Tuttle's voice from one tile to another one.
        /// </summary>
        [ProtoBuf.ProtoIgnore]
        protected const int _voiceSlideInterval = 500;

        /// <summary>
        /// Constructor
        /// </summary>
        public TuttleSoundComponent() : base()
            => _walkingVolume = .2f;

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case ReactToPinchingInDoor r: OnReactToPinchingInDoor(r); break;
                case ReactToCollision m: OnReactToCollision(m); break;
                case PositionChanged pc: OnPositionChanged(pc); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Handles the ReactToCollision message.
        /// </summary>
        /// <param name="m">The message to be handled.</param>
        private void OnReactToCollision(ReactToCollision m)
        {
            World.Sound.Play(stream: World.Sound.GetRandomSoundStream("movcrashdefault"), role: null, false, PositionType.Absolute, Owner.Area.Center.AsOpenALVector(), true, _defaultVolume);
            _movingSpeechHandle = World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_chipotleCollisionSound), role: null, false, PositionType.Absolute, Owner.Area.Center.AsOpenALVector(), true, _defaultVolume);
        }

        /// <summary>
        /// Handles the ReactOnPinchingInDoor message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnReactToPinchingInDoor(ReactToPinchingInDoor message)
            => _movingSpeechHandle = World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_doorPinchSound), role: null, false, PositionType.Absolute, Owner.Area.Center.AsOpenALVector(), true, _defaultVolume);

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnPositionChanged(PositionChanged message)
        {
            PlayTerrain(message.TargetPosition.Center, World.Map[message.TargetPosition.Center], message.Silently ? ObstacleType.Far : message.Obstacle);

            if (_movingSpeechHandle == 0)
                return;

            if (message.SourcePosition.Center == default) System.Diagnostics.Debugger.Break();

            _voiceSlidePosition = message.SourcePosition.Center.AsOpenALVector();
            _voiceSlideTarget = message.TargetPosition.Center.AsOpenALVector();
            Vector3 difference = _voiceSlideTarget - _voiceSlidePosition;
            _voiceSlideDelta = new Vector3(difference.X / _voiceSlideTicks, 0, difference.Z / _voiceSlideTicks);
            _voiceSlideTimer = 0;
        }

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
            float volume = _walkingVolume * 2;

            switch (obstacle)
            {
                case ObstacleType.Wall: lowpass = World.Sound.OverWallLowpass; volume = World.Sound.GetOverWallVolume(_walkingVolume); break;
                case ObstacleType.Door: lowpass = World.Sound.OverDoorLowpass; volume = World.Sound.GetOverDoorVolume(_walkingVolume); break;
                case ObstacleType.Object: lowpass = World.Sound.OverObjectLowpass; volume = World.Sound.GetOverObjectVolume(_walkingVolume); break;
            }

            // Play the sound
            string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
            int id = World.Sound.Play(stream: World.Sound.GetRandomSoundStream(soundName), role: null, looping: false, PositionType.Absolute, position.AsOpenALVector(), true, volume);

            if (attenuate)
                World.Sound.ApplyLowpass(id, lowpass);
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            DoVoiceSliding();
        }

        /// <summary>
        /// Performs sliding of Tuttle's voice during walk.
        /// </summary>
        private void DoVoiceSliding()
        {
            if (_movingSpeechHandle == 0)
                return;

            World.Sound.GetDynamicInfo(_movingSpeechHandle, out SoundState state, out var _);
            if (_voiceSlideTimer == -1 || state != SoundState.Playing)
                return;

            _voiceSlidePosition += _voiceSlideDelta;

            World.Sound.SetSourcePosition(_movingSpeechHandle, _voiceSlidePosition);

            if (++_voiceSlideTimer >= _voiceSlideTicks)
            {
                _voiceSlideTimer = -1;
                _voiceSlideDelta = default;
            }
        }
    }
}