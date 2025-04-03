using Game.Entities.Characters.Components;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Events.Characters;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Messaging.Events.Sound;
using Game.Terrain;

using ProtoBuf;

using System;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Tuttle
{
	/// <summary>
	/// Controls behavior of the Tuttle NPC
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class TuttleAI : AI
	{
		public override void Initialize() => transform.localScale = new(.4f, 1.7f, .4f);

		/// <summary>
		/// Timer that prevents repetetive collisions. Counts milliseconds.
		/// </summary>
		protected int _collisionTimer;

		/// <summary>
		/// Time limit between collisions (in milliseconds).
		/// </summary>
		protected int _collisionInterval = 2000;

		/// <summary>
		/// Specifies the minimum allowed distance from the Detective Chipotle NPC.
		/// </summary>
		/// <remarks>Used when following the Detective Chipotle NPC</remarks>
		private const float _minDistanceToPlayer = 2.5f;

		/// <summary>
		/// Specifies the maximum allowed distance from the Detective Chipotle NPC.
		/// </summary>
		/// <remarks>Used when following the Detective Chipotle NPC</remarks>
		private const int _maxDistanceToPlayer = 7;
		private const int _minDistanceToCar = 1;
		private const int _maxDistanceToCar = 2;

		/// <summary>
		/// Specifies if the NPC is just moving to another locality with the Chipotle's car.
		/// </summary>
		[ProtoIgnore]
		protected Locality _ridingTo;

		/// <summary>
		/// A delayed message of ChipotlesCarMoved type
		/// </summary>
		private ChipotlesCarMoved _carMovement;

		/// <summary>
		/// Indicates if the Detective Chipotle NPC visited the Walsch's pool (bazén w1) locality.
		/// </summary>
		private bool _playerWasByPool;

		/// <summary>
		/// Initializes the component and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();

			// Set position
			if (Settings.AllowTuttlesCustomPosition && Settings.TuttleTestStart.HasValue)
				JumpTo(Settings.TuttleTestStart.Value);
			else
				JumpTo(new Vector2(1031.8f, 1035.5f));

			// scenarios for debugging purposes
			if (!Settings.SendTuttleToPool && Settings.LetTuttleFollowChipotle)
				SetState(CharacterState.WatchingPlayer);
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case ObjectsCollided m: OnObjectsCollided(m); break;
				case PinchedInDoor h: OnPinchedInDoor(h); break;
				case CharacterStateChanged tsc: OnCharacterStateChanged(tsc); break;
				case ChipotlesCarMoved ccm: OnChipotlesCarMoved(ccm); break;
				case CutsceneEnded ce: OnCutsceneEnded(ce); break;
				case CutsceneBegan cb: OnCutsceneBegan(cb); break;
				case CharacterCameToLocality le: OnLocalityEntered(le); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Handles the Objectscolided message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected void OnObjectsCollided(ObjectsCollided message)
		{
			if (message.Sender != _player || message.Sender == _player && _collisionTimer < _collisionInterval)
				return;

			// Walk to the side a bit.
			GoTo(_player.Area.Value, _minDistanceToPlayer, _maxDistanceToPlayer);
			InnerMessage(new ReactToCollision(this, _player));
			_collisionTimer = 0;
		}

		/// <summary>
		/// Handles the HitByDoor message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnPinchedInDoor(PinchedInDoor message)
		{
			InnerMessage(new ReactToPinchingInDoor(this, message.Entity));
			GoTo(_player.Area.Value, _minDistanceToPlayer, _maxDistanceToPlayer);
		}

		/// <summary>
		/// Processes the CutsceneBegan message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnCutsceneBegan(CutsceneBegan message)
		{
			base.OnCutsceneBegan(message);

			switch (message.CutsceneName)
			{
				case "cs7": case "cs8": Reveal(); break;
				case "cs14": StopFollowing(); break;
				case "cs19": Hide(); break;
				case "cs21": case "cs23": StopFollowing(); break;
			}
		}

		/// <summary>
		/// Processes the CutsceneEnded message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnCutsceneEnded(CutsceneEnded message)
		{
			base.OnCutsceneEnded(message);
			WatchCar();

			switch (message.CutsceneName)
			{
				case "cs6": GoToPool(); break;
				case "cs14": JumpToBelvedereStreet(); break;
				case "cs21": JumpToChristinesHall(); break;
				case "cs23": JumpToSweeneysRoom(); break;
				case "cs38": StartFollowingPlayer(); break;
			}
		}

		/// <summary>
		/// Instructs the NPC to walk towards the corpse (tělo w1) object and wait there for the
		/// Detective Chipotle NPC.
		/// </summary>
		private void GoToPool()
		{
			if (!Settings.SendTuttleToPool)
				return;

			Vector2 goal = new(1005, 1051);
			GoToPoint(goal);
		}

		/// <summary>
		/// Indicates that the player moved from his original location to the bazén w1 locality. It also means that he walked past the Tuttle NPC because 
		/// </summary>
		private bool _playerWasByThePool;

		/// <summary>
		/// The Detective Chipotle and Tuttle NPCs relocate from the Walsch's drive way (příjezdová
		/// cesta w1) locality to the Belvedere street (ulice p1) locality right outside the
		/// Christine's door.
		/// </summary>
		private void JumpToBelvedereStreet()
		{
			JumpNear(World.GetItem("zvonek p1").Area.Value);
			StartFollowingPlayer();
		}

		/// <summary>
		/// The detective Chipotle and Tuttle NPCs relocate from the Belvedere street (ulice p1)
		/// locality to the Christine's hall (hala p1) locality.
		/// </summary>
		private void JumpToChristinesHall() => JumpNear(World.GetItem("botník p1").Area.Value);

		/// <summary>
		/// Stops following the player.
		/// </summary>
		private void StopFollowing() => InnerMessage(new StopFollowingPlayer(this));

		/// <summary>
		/// The Tuttle and Sweeney NPCs relocate from the Sweeney's hall (hala s1) locality to his
		/// room (pokoj s1) locality.
		/// </summary>
		private void JumpToSweeneysRoom() => JumpNear(World.GetItem("skříň s2").Area.Value);

		/// <summary>
		/// Processes the ChipotlesCarMoved message.
		/// </summary>
		/// <param name="m">The message to be processed</param>
		private void OnChipotlesCarMoved(ChipotlesCarMoved m)
		{
			Locality locality = m.Target.GetLocalities().First();

			if (locality.Name.Indexed != "asfaltka c1")
				_carMovement = m;

			_ridingTo = _carMovement.Target.GetLocalities().First();
			InnerMessage(new StopFollowingPlayer(this)); // This tells the NPC to stop following the Chipotle NPC till they both arrive to new locality.
		}

		/// <summary>
		/// Processes the LocalityEntered message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnLocalityEntered(CharacterCameToLocality message)
		{
			// When the player first time enters the locality start following him.
			if (message.Character == _player && Owner.Locality.Name.Indexed == "bazén w1" && !_playerWasByPool)
			{
				_playerWasByPool = true;

				if (Settings.LetTuttleFollowChipotle)
					InnerMessage(new StartFollowingPlayer(this));
			}
		}

		/// <summary>
		/// Makes the NPC visible to the other NPCs and objects.
		/// </summary>
		protected void Reveal()
		{
			Vector2? target = FindFreePlacementsAroundArea(_player.Area.Value, _minDistanceToPlayer, _maxDistanceToPlayer)
				.FirstOrDefault();

			if (target == null)
				throw new ArgumentNullException("No walkable tile near player");
			base.Reveal(target.Value);
		}

		/// <summary>
		/// Relocates the NPC near the car of the Detective Chipotle NPC if the car has moved recently.
		/// </summary>
		private void WatchCar()
		{
			if (_hidden || _carMovement == null)
				return;

			float height = transform.localScale.y;
			float width = transform.localScale.x;
			Vector2? target = World.FindFreePlacementsAroundArea(null, _carMovement.Target, height, width, _minDistanceToCar, _maxDistanceToCar)
				.FirstOrDefault();
			if (target == null)
				throw new ArgumentNullException("No walkable tile found.");

			InnerMessage(new SetPosition(this, new((Vector2)target), true));
			_carMovement = null;
		}

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();
			WaitForPlayer();
			WatchTimers();
		}

		/// <summary>
		/// Watches and sets timers.
		/// </summary>
		protected void WatchTimers()
		{
			if (_collisionTimer < _collisionInterval)
				_collisionTimer += World.DeltaTime;
		}

		/// <summary>
		/// Restarts following the Chipotle NPC if they both arrived to a new locality by car.
		/// </summary>
		private void WaitForPlayer()
		{
			if (_ridingTo != null && Owner.Locality == _ridingTo && World.Player.Locality == _ridingTo)
			{
				_ridingTo = null;
				InnerMessage(new StartFollowingPlayer(this));
			}
		}
	}
}