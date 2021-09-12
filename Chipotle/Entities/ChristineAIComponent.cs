using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using System;
using System.Collections.Generic;




namespace Game.Entities
{
    public class ChristineAIComponent : AIComponent
    {
        protected override void OnCutsceneEnded(CutsceneEnded message)
        {
            base.OnCutsceneEnded(message);

            switch (message.CutsceneName)
            {
                case "cs21": JumpToBedroom(); break;
            }
        }

        private void JumpToBedroom()
            => Owner.ReceiveMessage(new SetPosition(this, new Plane("1744, 1123"), true));

        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
                    [typeof(CutsceneEnded)] = (message) => OnCutsceneEnded((CutsceneEnded)message),
                }
                );

            Owner.ReceiveMessage(new SetPosition(this, new Plane("1792, 1127"), true));
        }
    }
}
