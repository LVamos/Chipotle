using Game.Terrain;
using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;

using Luky;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Game.Entities
{
    public class SweeneyAIComponent : AIComponent
    {
        protected override void OnCutsceneEnded(CutsceneEnded message)
        {
             base.OnCutsceneEnded(message);

            switch (message.CutsceneName)
            {
                case "cs23": JumpToSweeneysRoom(); break;
            }

        }

        private void JumpToSweeneysRoom()
            => Owner.ReceiveMessage(new SetPosition(this, new Plane("1407, 978"), true));

        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>()
                {
                    [typeof(CutsceneEnded)] = (message) => OnCutsceneEnded((CutsceneEnded)message)
                }
                );

            Owner.ReceiveMessage(new SetPosition(this, new Plane("1402, 960"), true));
        }
    }
}
