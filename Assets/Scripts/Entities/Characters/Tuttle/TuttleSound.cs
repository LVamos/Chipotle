using Game.Audio;
using Game.Entities.Characters.Components;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Events.Movement;
using Game.Terrain;



using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Tuttle
{
	/// <summary>
	/// Controls the sound output of the Tuttle NPC
	/// </summary>
	
	public class TuttleSound : Sound
	{

		/// <summary>
		/// A sound played when Tuttle gets pinched in a door by another NPC.
		/// </summary>
		protected const string _doorPinchSound = "TuttleHitByDoor";

		/// <su mmary>
		/// Handle of sound with Tuttle's speech that can move when Tuttle is walking.
		/// </summary>
		
		protected AudioSource _movingSpeechAudio;

		/// <summary>
		/// Target position for current Tuttle's voice sliding.
		/// </summary>
		
		protected Vector3 _voiceSlideTarget;

		/// <summary>
		/// Current position for Tuttle's voice sliding.
		/// </summary>
		protected Vector3 _voiceSlidePosition;

		/// <summary>
		/// Position the current voice slide started at.
		/// </summary>
		protected Vector3 _voiceSlideSource;

		/// <summary>
		/// Time elapsed since the voice slide started, in seconds; negative when no slide is running.
		/// </summary>
		protected float _voiceSlideTime = -1;

		/// <summary>
		/// Sound with reaction on collision with Chipotle.
		/// </summary>
		private const string _chipotleCollisionSound = "TuttleHitByDoor";

		/// <summary>
		/// Specifies how long it takes to slide position of Tuttle's voice from one tile to another one, in seconds.
		/// </summary>
		protected const float _voiceSlideDuration = .5f;

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
		/// <param name="message">The message to be handled.</param>
		private void OnReactToCollision(ReactToCollision message)
		{
			Sounds.Play("movcrashdefault", transform.position, _defaultVolume);
			PlayMovingVoice();
		}

		private const string _movingSound = "TuttleHitByDoor";

		private void PlayMovingVoice()
		{
			_voiceSlidePosition = transform.position;
			_movingSpeechAudio = Sounds.Play(_movingSound, _voiceSlidePosition);
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
			if (message.Silently)
				return;

			Vector2 position = message.TargetPosition.Center;
			ObstacleType obstacle = message.Obstacle;
			PlayStep(position, obstacle);
			RegisterStep(message.StepDuration, () => PlayStep(position, obstacle));

			// Slide voice of the NPC if talking
			if (_movingSpeechAudio == null || !_movingSpeechAudio.isPlaying)
				return;

			float height = transform.localScale.y;
			Vector3 source = message.SourcePosition.Value.Center.ToVector3(height);
			_voiceSlideSource = source;
			_voiceSlidePosition = source;
			_voiceSlideTarget = message.TargetPosition.Center.ToVector3(height);
			_voiceSlideTarget.y = source.y;
			_voiceSlideTime = 0;
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
			if (_voiceSlideTime < 0 || _movingSpeechAudio == null || !_movingSpeechAudio.isPlaying)
				return;

			_voiceSlideTime += Time.deltaTime;
			_voiceSlidePosition = Vector3.Lerp(_voiceSlideSource, _voiceSlideTarget, _voiceSlideTime / _voiceSlideDuration);
			_movingSpeechAudio.transform.position = _voiceSlidePosition;

			if (_voiceSlideTime >= _voiceSlideDuration)
				_voiceSlideTime = -1;
		}
	}
}