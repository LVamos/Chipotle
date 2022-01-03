using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Controls behavior of the Tuttle NPC
    /// </summary>
    [Serializable]
    public class TuttleAIComponent : AIComponent
    {
        /// <summary>
        /// Reference to the Detective Chipotle NPC
        /// </summary>
        private readonly Entity _player = World.Player;

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

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
                    [typeof(ChipotlesCarMoved)] = (message) => OnChipotlesCarMoved((ChipotlesCarMoved)message),
                    [typeof(CutsceneBegan)] = (message) => OnCutsceneBegan((CutsceneBegan)message),
                    [typeof(CutsceneEnded)] = (message) => OnCutsceneEnded((CutsceneEnded)message),
                    [typeof(LocalityEntered)] = (m) => OnLocalityEntered((LocalityEntered)m),
                    [typeof(CutsceneEnded)] = (m) => OnCutsceneEnded((CutsceneEnded)m)
                }
                );

            Owner.ReceiveMessage(new SetPosition(this, new Plane(new Vector2(1030, 1036)), true));
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
                case "cs6": GoToCorpse(); break;
                case "cs14": JumpToBelvedereStreet2(); break;
                case "cs21": JumpToChristinesHall(); break;
                case "cs23": JumpToSweeneysRoom(); break;
            }
        }

        /// <summary>
        /// Instructs the NPC to walk towards the corpse (tělo w1) object and wait there for the
        /// Detective Chipotle NPC.
        /// </summary>
        private void GoToCorpse()
        {
            Vector2 goal = new Vector2(936, 1059);
            Queue<Vector2> path =
            _finder.FindPath(_area.Center, goal)
            ?? throw new InvalidOperationException(nameof(OnCutsceneEnded));
            Owner.ReceiveMessage(new StopFollowing(this));
            Owner.ReceiveMessage(new GotoPoint(this, path, goal, 400));
        }

        /// <summary>
        /// Makes the NPC invisible for the other NPCs and objects.
        /// </summary>
        private void Hide()
        {
            _hidden = true;
            Owner.ReceiveMessage(new Hide(this));
        }

        /// <summary>
        /// The Detective Chipotle and Tuttle NPCs relocate from the Walsch's drive way (příjezdová
        /// cesta w1) locality to the Belvedere street (ulice p1) locality right outside the
        /// Christine's door.
        /// </summary>
        private void JumpToBelvedereStreet2()
        {
            SetPosition message = new SetPosition(this, new Plane("1806, 1121"), true);
            Owner.ReceiveMessage(message);
        }

        /// <summary>
        /// The detective Chipotle and Tuttle NPCs relocate from the Belvedere street (ulice p1)
        /// locality to the Christine's hall (hala p1) locality.
        /// </summary>
        private void JumpToChristinesHall()
            => Owner.ReceiveMessage(new SetPosition(this, new Plane("1791, 1124"), true));

        /// <summary>
        /// The Tuttle and Sweeney NPCs relocate from the Sweeney's hall (hala s1) locality to his
        /// room (pokoj s1) locality.
        /// </summary>
        private void JumpToSweeneysRoom()
            => Owner.ReceiveMessage(new SetPosition(this, new Plane("1411, 974"), true));

        /// <summary>
        /// Processes the ChipotlesCarMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnChipotlesCarMoved(ChipotlesCarMoved message)
        {
            if (message.Target.GetLocality().Name.Indexed != "asfaltka c1")
                _carMovement = message;
        }

        /// <summary>
        /// Processes the LocalityEntered message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnLocalityEntered(LocalityEntered message)
        {
            if (message.Entity== _player && Owner.Locality.Name.Indexed == "bazén w1" && !_playerWasByPool)
            {
                _playerWasByPool = true;
                Owner.ReceiveMessage(new StartFollowing(this));
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
            Owner.ReceiveMessage(new Reveal(this, new Plane((Vector2)target)));
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
            Owner.ReceiveMessage(new SetPosition(this, new Plane((Vector2)target), true));
            _carMovement = null;
        }
    }
}