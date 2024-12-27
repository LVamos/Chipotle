using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using OpenTK;

using ProtoBuf;

using System.Collections.Generic;
using System.Linq;

namespace Game.Entities
{
    /// <summary>
    /// Controls behavior of the Tuttle NPC
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class TuttleAIComponent : AIComponent
    {
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
        private const int _minDistanceFromPlayer = 6;

        /// <summary>
        /// Specifies the maximum allowed distance from the Detective Chipotle NPC.
        /// </summary>
        /// <remarks>Used when following the Detective Chipotle NPC</remarks>
        private const int _maxDistanceFromPlayer = 10;

        /// <summary>
        /// Specifies if the NPC is just moving to another locality with the Chipotle's car.
        /// </summary>
        [ProtoIgnore]
        protected Locality _ridingTo;

        /// <summary>
        /// Handles the TuttleStateChanged message.
        /// </summary>
        /// <param name="message">The message</param>
        private void OnTuttleStateChanged(TuttleStateChanged message)
=> _state = message.State;

        /// <summary>
        /// sets state of the NPC and announces the change to other components.
        /// </summary>
        private void SetState(TuttleState state)
        {
            _state = state;
            InnerMessage(new TuttleStateChanged(this, state));
        }

        /// <summary>
        /// Indicates what the NPC is doing in the moment.
        /// </summary>
        private TuttleState _state = TuttleState.Waiting;

        /// <summary>
        /// Reference to the Detective Chipotle NPC
        /// </summary>
        [ProtoIgnore]
        private Character _player => World.Player;

        /// <summary>
        /// A delayed message of ChipotlesCarMoved type
        /// </summary>
        private ChipotlesCarMoved _carMovement;

        /// <summary>
        /// Indicates if the NPC is invisible for other NPCs and objects.
        /// </summary>
        private bool _hidden;

        /// <summary>
        /// Indicates if the Detective Chipotle NPC visited the Walsch's pool (bazén w1) locality.
        /// </summary>
        private bool _playerWasByPool;

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Set position
            if (Program.Settings.AllowTuttlesCustomPosition && Program.Settings.TuttleTestStart.HasValue)
                _area = new Rectangle((Vector2)Program.Settings.TuttleTestStart);
            else _area = new Rectangle(new Vector2(1029, 1039));
            InnerMessage(new SetPosition(this, new Rectangle(_area), true));

            // scenarios for debugging purposes
            //if (Program.Settings.SendTuttleToPool && !Program.Settings.PlayCutscenes)
            //    GoToPool();
            if (!Program.Settings.SendTuttleToPool && Program.Settings.LetTuttleFollowChipotle)
                SetState(TuttleState.WatchingPlayer);
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case ObjectsCollided m: OnObjectsCollided(m); break;
                case PinchedInDoor h: OnPinchedInDoor(h); break;
                case TuttleStateChanged tsc: OnTuttleStateChanged(tsc); break;
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
        /// <param name="m">The message to be handled</param>
        protected void OnObjectsCollided(ObjectsCollided m)
        {
            if (m.Sender != _player || (m.Sender == _player && _collisionTimer < _collisionInterval))
                return;

            // Walk to the side a bit.
            GoAwayFrom(_area, 2, _maxDistanceFromPlayer);
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
            GoAwayFrom(message.Sender.Area);
        }

        /// <summary>
        /// Tuttle makes few random steps in the current locality.
        /// </summary>
        protected void GoAwayFrom(Rectangle area, int minDistance = 4, int maxDistance = 10)
        {
            // Go away from the door. Find a random walkable tile near the player in the same locality where he's standing.
            Rectangle aroundPlayer = new Rectangle(_player.Area);
            aroundPlayer.Extend(4);

            // Find walkable tiles around the player.
            Vector2[] walkables =
 (from p in _player.Area.GetWalkableSurroundingPoints(minDistance, maxDistance)
  let distance = area.GetDistanceFrom(p)
  where _player.Locality.Area.Intersects(p) && distance >= minDistance && distance <= maxDistance
  orderby _player.Area.GetDistanceFrom(p)
  select p)
  .Take(20)
  .ToArray<Vector2>();

            // Tuttle tries each point from the array.
            if (walkables != null)
                TryGoTo(walkables);
        }

        protected void TryGoTo(Vector2[] points, bool watchPlayer = false)
=> InnerMessage(new TryGoTo(this, points, watchPlayer));
        /// <summary>
        /// Sends Tuttle to the specified point.
        /// </summary>
        /// <param name="point">The target point</param>
        /// <param name="watchPlayer">Specifies if Tuttle should stop following the player while leading to the target</param>
        protected void GoToPoint(Vector2 point, bool watchPlayer = false)
            => InnerMessage(new GotoPoint(this, point, watchPlayer));

        /// <summary>
        /// Sends Tuttle on the specified path.
        /// </summary>
        /// <param name="path">The path to be folloewd</param>
        protected void FollowPath(Queue<Vector2> path)
            => InnerMessage(new FollowPath(this, path));

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
                case "cs14": JumpToBelvedereStreet2(); break;
                case "cs21": JumpToChristinesHall(); break;
                case "cs23": JumpToSweeneysRoom(); break;
                case "cs38": StartFollowing(); break;
            }
        }

        /// <summary>
        /// Instructs the NPC to walk towards the corpse (tělo w1) object and wait there for the
        /// Detective Chipotle NPC.
        /// </summary>
        private void GoToPool()
        {
            if (!Program.Settings.SendTuttleToPool)
                return;

            Vector2 goal = new Vector2(1005, 1051);
            GoToPoint(goal);
        }

        /// <summary>
        /// Indicates that the player moved from his original location to the bazén w1 locality. It also means that he walked past the Tuttle NPC because 
        /// </summary>
        private bool _playerWasByThePool;

        /// <summary>
        /// Makes the NPC invisible for the other NPCs and objects.
        /// </summary>
        private void Hide()
        {
            _hidden = true;
            InnerMessage(new Hide(this));
        }

        /// <summary>
        /// The Detective Chipotle and Tuttle NPCs relocate from the Walsch's drive way (příjezdová
        /// cesta w1) locality to the Belvedere street (ulice p1) locality right outside the
        /// Christine's door.
        /// </summary>
        private void JumpToBelvedereStreet2()
        {
            InnerMessage(new SetPosition(this, new Rectangle("1801, 1124"), true));
            StartFollowing();
        }

        /// <summary>
        /// The detective Chipotle and Tuttle NPCs relocate from the Belvedere street (ulice p1)
        /// locality to the Christine's hall (hala p1) locality.
        /// </summary>
        private void JumpToChristinesHall()
            => InnerMessage(new SetPosition(this, new Rectangle("1791, 1124"), true));

        /// <summary>
        /// Starts following the player.
        /// </summary>
        private void StartFollowing()
        => InnerMessage(new StartFollowing(this));

        /// <summary>
        /// Stops following the player.
        /// </summary>
        private void StopFollowing()
            => InnerMessage(new StopFollowing(this));

        /// <summary>
        /// The Tuttle and Sweeney NPCs relocate from the Sweeney's hall (hala s1) locality to his
        /// room (pokoj s1) locality.
        /// </summary>
        private void JumpToSweeneysRoom()
            => InnerMessage(new SetPosition(this, new Rectangle("1411, 974"), true));

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
            InnerMessage(new StopFollowing(this)); // This tells the NPC to stop following the Chipotle NPC till they both arrive to new locality.
        }

        /// <summary>
        /// Processes the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityEntered(CharacterCameToLocality message)
        {
            // When the player first time enters the loclaity start following him.
            if (message.Character == _player && Owner.Locality.Name.Indexed == "bazén w1" && !_playerWasByPool)
            {
                _playerWasByPool = true;

                if (Program.Settings.LetTuttleFollowChipotle)
                    InnerMessage(new StartFollowing(this));
            }
        }

        /// <summary>
        /// Makes the NPC visible to the other NPCs and objects.
        /// </summary>
        private void Reveal()
        {
            Vector2? target = World.GetRandomWalkablePoint(_player, _minDistanceFromPlayer, _maxDistanceFromPlayer);
            Assert(target.HasValue, "No walkable tile near player");

            _hidden = false;
            InnerMessage(new Reveal(this, new Rectangle((Vector2)target)));
        }

        /// <summary>
        /// Relocates the NPC near the car of the Detective Chipotle NPC if the car has moved recently.
        /// </summary>
        private void WatchCar()
        {
            if (_hidden || _carMovement == null)
                return;

            Vector2? target = World.GetRandomWalkablePoint(_carMovement.Target, 1, 2);
            Assert(target.HasValue, "No walkable tile found.");
            InnerMessage(new SetPosition(this, new Rectangle((Vector2)target), true));
            _carMovement = null;
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            WaitForChipotle();
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
        private void WaitForChipotle()
        {
            if (_ridingTo != null && Owner.Locality == _ridingTo && World.Player.Locality == _ridingTo)
            {
                _ridingTo = null;
                InnerMessage(new StartFollowing(this));
            }
        }
    }
}