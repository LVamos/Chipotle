using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;

namespace Game.Entities
{
    public class TuttleAIComponent : AIComponent
    {
        private bool _hidden;
        private bool _playerWasByPool;

        /// <summary>
        /// Jumps just before pub. Chiptole should do the same in the same time.
        /// </summary>
        private void JumpToPub()
        {
            SetPosition message = new SetPosition(this, new Plane("1552, 1014"), true);
            Owner.ReceiveMessage(message);
        }




        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
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

            switch (message.CutsceneName)
            {
                case "cs6": GoToCorpse(); break;
                case "cs8": JumpToPub(); break;
                case "cs14": JumpToBelvedereStreet(); break;
                case "cs19": Hide(); break;
            }
        }

        private void Hide()
        {
            _hidden = true;
            Owner.ReceiveMessage(new Hide(this));
        }

        /// <summary>
        /// Chiipotle and Tuttle get out to Belvedere street right in front of Christine's front door.
        /// </summary>
        private void JumpToBelvedereStreet()
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
