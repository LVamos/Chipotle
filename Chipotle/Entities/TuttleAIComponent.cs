using ProtoBuf;
using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using OpenTK;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Controls behavior of the Tuttle NPC
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class TuttleAIComponent : AIComponent
    {
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
        private TuttleState _state    = TuttleState.Waiting;

        /// <summary>
        /// Reference to the Detective Chipotle NPC
        /// </summary>
        [ProtoIgnore]
        private Entity _player => World.Player;

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
            {
                _area = new Plane((Vector2)Program.Settings.TuttleTestStart);

                if (Program.Settings.LetTuttleFollowChipotle)
                    SetState(TuttleState.WatchingPlayer);
            }
            else _area = new Plane(new Vector2(1030, 1036));
            InnerMessage(new SetPosition(this, new Plane(_area), true));
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                                case TuttleStateChanged tsc:OnTuttleStateChanged(tsc); break;
                    case ChipotlesCarMoved ccm:OnChipotlesCarMoved(ccm); break;
                case CutsceneEnded ce: OnCutsceneEnded(ce); break;
                    case CutsceneBegan cb:OnCutsceneBegan(cb); break;
                case LocalityEntered le: OnLocalityEntered(le); break;
                default: base.HandleMessage(message); break;
            }
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
                case "cs19": Hide(); break;
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
            InnerMessage(new GotoPoint(this, goal));
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
            SetPosition message = new SetPosition(this, new Plane("1806, 1121"), true);
            InnerMessage(message);
        }

        /// <summary>
        /// The detective Chipotle and Tuttle NPCs relocate from the Belvedere street (ulice p1)
        /// locality to the Christine's hall (hala p1) locality.
        /// </summary>
        private void JumpToChristinesHall()
            => InnerMessage(new SetPosition(this, new Plane("1791, 1124"), true));

        /// <summary>
        /// The Tuttle and Sweeney NPCs relocate from the Sweeney's hall (hala s1) locality to his
        /// room (pokoj s1) locality.
        /// </summary>
        private void JumpToSweeneysRoom()
            => InnerMessage(new SetPosition(this, new Plane("1411, 974"), true));

        /// <summary>
        /// Processes the ChipotlesCarMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnChipotlesCarMoved(ChipotlesCarMoved message)
        {
            if (message.Target.GetLocality().Name.Indexed != "asfaltka c1")
                _carMovement = message;

            _ridingTo = _carMovement.Target.GetLocality();
            InnerMessage(new StopFollowing(this)); // This tells the NPC to stop following the Chipotle NPC till they both arrive to new locality.
        }

        /// <summary>
        /// Processes the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityEntered(LocalityEntered message)
        {
            // When the player first time enters the loclaity start following him.
            if (message.Entity== _player && Owner.Locality.Name.Indexed == "bazén w1" && !_playerWasByPool)
            {
                _playerWasByPool = true;

                if(Program.Settings.LetTuttleFollowChipotle)
                InnerMessage(new StartFollowing(this));
            }
        }

        /// <summary>
        /// Makes the NPC visible to the other NPCs and objects.
        /// </summary>
        private void Reveal()
        {
            Vector2? target = _player.Area.FindNearestWalkableTile(10);
            Assert(target.HasValue, "No walkable tile near player");

            _hidden = false;
            InnerMessage(new Reveal(this, new Plane((Vector2)target)));
        }

        /// <summary>
        /// Relocates the NPC near the car of the Detective Chipotle NPC if the car has moved recently.
        /// </summary>
        private void WatchCar()
        {
            if (_hidden || _carMovement == null)
                return;

            Plane perimeter = new Plane(_carMovement.Target);
            perimeter.Extend();
            Vector2? target = perimeter.FindRandomWalkableTile(1);
            Assert(target.HasValue, "No walkable tile found.");
            InnerMessage(new SetPosition(this, new Plane((Vector2)target), true));
            _carMovement = null;
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            WaitForChipotle();
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