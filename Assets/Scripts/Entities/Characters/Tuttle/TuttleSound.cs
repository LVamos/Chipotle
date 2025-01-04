using Game.Audio;
using Game.Entities.Characters.Components;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Events.Movement;
using Game.Terrain;

using ProtoBuf;

using System;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Tuttle
{
	/// <summary>
	/// Controls the sound output of the Tuttle NPC
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class TuttleSound : Sound
	{
		private GameObject _movingSpeechObject;

		/// <summary>
		/// A sound played when Tuttle gets pinched in a door by another NPC.
		/// </summary>
		protected const string _doorPinchSound = "TuttleHitByDoor";

		/// <su mmary>
		/// Handle of sound with Tuttle's speech that can move when Tuttle is walking.
		/// </summary>
		[ProtoIgnore]
		protected AudioSource _movingSpeechAudio;

		/// <summary>
		/// Target position for current Tuttle's voice sliding.
		/// </summary>
		[ProtoIgnore]
		protected Vector3 _voiceSlideTarget;

		/// <summary>
		/// Current position for Tuttle's voice sliding.
		/// </summary>
		protected Vector3 _voiceSlidePosition;

		/// <summary>
		/// Specifies how the current position of Tuttle's voice changes in one step when voice sliding is being performed.
		/// </summary>
		[ProtoIgnore]
		protected Vector3 _voiceSlideDelta;

		/// <summary>
		/// Specifies how much steps are needed to perform the voice slide.
		/// </summary>
		protected int _voiceSlideTicks => _voiceSlideInterval / World.DeltaTime;

		/// <summary>
		/// A counter used for voice sliding.
		/// </summary>
		[ProtoIgnore]
		protected int _voiceSlideTimer = -1;

		/// <summary>
		/// Sound with reaction on collision with Chipotle.
		/// </summary>
		private const string _chipotleCollisionSound = "TuttleHitByDoor";

		/// <summary>
		/// Specifies how long it takes to slide position of Tuttle's voice from one tile to another one.
		/// </summary>
		[ProtoIgnore]
		protected const int _voiceSlideInterval = 500;

		/// <summary>
		/// Constructor
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			_walkVolume = .2f;
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
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
			Sounds.Play("movcrashdefault", Owner.Area.Value.Center, _defaultVolume);
			PlayMovingVoice();
		}

		private void PlayMovingVoice()
		{
			_movingSpeechObject.transform.position = Owner.Area.Value.Center;
			_movingSpeechAudio.Play();
		}

		/// <summary>
		/// Handles the ReactOnPinchingInDoor message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnReactToPinchingInDoor(ReactToPinchingInDoor message)
		{
			PlayMovingVoice();
		}

		/// <summary>
		/// Processes the message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnPositionChanged(PositionChanged message)
		{
			Vector2 target = message.TargetPosition.Center;
			Tile terrain = World.Map[target];
			PlayTerrain(target, terrain, message.Silently ? ObstacleType.Far : message.Obstacle);

			if (_movingSpeechAudio == null || !_movingSpeechAudio.isPlaying)
				return;

			Vector2 source = message.SourcePosition.Value.Center;
			_voiceSlidePosition = source;
			_voiceSlideTarget = target;
			Vector3 difference = _voiceSlideTarget - _voiceSlidePosition;
			_voiceSlideDelta = new(difference.x / _voiceSlideTicks, 0, difference.z / _voiceSlideTicks);
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
			bool attenuate = obstacle is not ObstacleType.None and not ObstacleType.IndirectPath;
			;
			float volume = _walkVolume * 2;

			switch (obstacle)
			{
				case ObstacleType.Wall:
					volume = Sounds.GetOverWallVolume(_walkVolume);
					break;
				case ObstacleType.Door:
					volume = Sounds.GetOverDoorVolume(_walkVolume);
					break;
				case ObstacleType.Object:
					volume = Sounds.GetOverObjectVolume(_walkVolume);
					break;
			}

			string soundName = "movstep" + Enum.GetName(tile.Terrain.GetType(), tile.Terrain);
			Sounds.Play(soundName, position.ToVector3(_footStepHeight), volume);
		}

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();
			DoVoiceSliding();
		}

		/// <summary>
		/// Performs sliding of Tuttle's voice during walk.
		/// </summary>
		private void DoVoiceSliding()
		{
			if (_voiceSlideTimer == -1 || !_movingSpeechAudio.isPlaying)
				return;

			_voiceSlidePosition += _voiceSlideDelta;
			_movingSpeechObject.transform.position = _voiceSlideDelta;

			if (++_voiceSlideTimer >= _voiceSlideTicks)
			{
				_voiceSlideTimer = -1;
				_voiceSlideDelta = default;
			}
		}
	}
}