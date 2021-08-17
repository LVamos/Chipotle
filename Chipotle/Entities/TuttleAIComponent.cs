using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    public class TuttleAIComponent : AIComponent
    {
        private bool _hidden;
        private bool _playerWasByPool;
        private ChipotlesCarMoved _carMovement;


        private void OnChipotlesCarMoved(ChipotlesCarMoved message)
=> _carMovement = message;

        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
                    [typeof(ChipotlesCarMoved)] = (message) => OnChipotlesCarMoved((ChipotlesCarMoved)message),
                    [typeof(CutsceneEnded)] = (message) => OnCutsceneEnded((CutsceneEnded)message),
                    [typeof(LocalityEntered)] = (m) => OnLocalityEntered((LocalityEntered)m),
                    [typeof(CutsceneEnded)] = (m) => OnCutsceneEnded((CutsceneEnded)m)
                }
                );

            Owner.ReceiveMessage(new SetPosition(this, new Plane(new Vector2(1030, 1036)), true));
        }

        private readonly Entity _player = World.Player;
        private void OnLocalityEntered(LocalityEntered m)
        {
            if (m.Sender == _player && _area.GetLocality().Name.Indexed == "bazén w1" && !_playerWasByPool)
            {
                _playerWasByPool = true;
                Owner.ReceiveMessage(new StartFollowing(this));
            }
        }

        protected override void OnCutsceneEnded(CutsceneEnded message)
        {
            base.OnCutsceneEnded(message);
            WatchCar();


            switch (message.CutsceneName)
            {
                case "cs6": GoToCorpse(); break;
                case "cs14": JumpToBelvedereStreet2(); break;
                case "cs19": Hide(); break;
                case "cs21": JumpToChristinesHall(); break;
                case "cs23": JumpToSweeneysRoom(); break;
            }
        }

        private void WatchCar()
        {
            if (_hidden || _carMovement == null)
                return;

            Vector2? target = _carMovement.TargetLocation.FindRandomWalkableTile(1);
            Assert(target.HasValue, "No walkable tile found.");
            Owner.ReceiveMessage(new SetPosition(this, new Plane((Vector2)target), true));
            _carMovement = null;
        }
        private void JumpToSweeneysRoom()
            => Owner.ReceiveMessage(new SetPosition(this, new Plane("1411, 974"), true));


        private void JumpToChristinesHall()
            => Owner.ReceiveMessage(new SetPosition(this, new Plane("1791, 1124"), true));

        private void Hide()
        {
            _hidden = true;
            Owner.ReceiveMessage(new Hide(this));
        }

        /// <summary>
        /// Chiipotle and Tuttle get out to Belvedere street right in front of Christine's front door.
        /// </summary>
        private void JumpToBelvedereStreet2()
        {
            SetPosition message = new SetPosition(this, new Plane("1806, 1121"), true);
            Owner.ReceiveMessage(message);
        }

        /// <summary>
        /// Instructs Tuttle to get to corpse and wait there for Chipotle
        /// </summary>
        private void GoToCorpse()
        {
            Queue<Vector2> path =
                _finder.FindPath(_area.Center, new Vector2(936, 1059))
                ?? throw new InvalidOperationException(nameof(OnCutsceneEnded));
            Owner.ReceiveMessage(new StopFollowing(this));
            Owner.ReceiveMessage(new GotoPoint(this, path, 400));
        }
    }
}
