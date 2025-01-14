using Game.Audio;
using Game.Entities.Characters.Components;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Events.Movement;

using ProtoBuf;

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

			PlayStep(message.TargetPosition.Center, message.Obstacle);

			// Slide voice of the NPC if talking
			if (_movingSpeechAudio == null || !_movingSpeechAudio.isPlaying)
				return;

			float height = transform.localScale.y;
			Vector3 source = message.SourcePosition.Value.Center.ToVector3(height);
			_voiceSlidePosition = source;
			_voiceSlideTarget = message.TargetPosition.Center.ToVector3(height);
			Vector3 difference = _voiceSlideTarget - _voiceSlidePosition;
			_voiceSlideDelta = new(difference.x / _voiceSlideTicks, 0, difference.z / _voiceSlideTicks);
			_voiceSlideTimer = 0;
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
			if (_voiceSlideTimer == -1 || _movingSpeechAudio is { isPlaying: false })
				return;

			_voiceSlidePosition += _voiceSlideDelta;
			_movingSpeechAudio.transform.position = _voiceSlidePosition;

			if (++_voiceSlideTimer >= _voiceSlideTicks)
			{
				_voiceSlideTimer = -1;
				_voiceSlideDelta = default;
			}
		}
	}
}