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
        private bool _playerWasByPool;

        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
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

        protected override void OnCutsceneEnded(CutsceneEnded m)
        {
            base.OnCutsceneEnded(m);

            if (m.CutsceneName == "cs6")
            {
                // Go to corpse and wait there for Chipotle
                Queue<Vector2> path = _finder.FindPath(_area.Center, new Vector2(936, 1059)) ?? throw new InvalidOperationException(nameof(OnCutsceneEnded));
                Owner.ReceiveMessage(new StopFollowing(this));
                Owner.ReceiveMessage(new GotoPoint(this, path, 400));
            }
        }
    }
}
