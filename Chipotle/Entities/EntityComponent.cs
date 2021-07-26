using Game.Messaging;
using Game.Messaging.Events;

using Luky;

using System;
using System.Collections.Generic;

namespace Game.Entities
{
    public abstract class EntityComponent : MessagingObject
    {







        public Entity Owner;
        protected bool _cutsceneInProgress;

        public new Name Name => Owner?.Name;

        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>()
                {
                    [typeof(CutsceneBegan)] = (m) => OnCutsceneBegan((CutsceneBegan)m),
                    [typeof(CutsceneEnded)] = (m) => OnCutsceneEnded((CutsceneEnded)m)

                }
                );
        }

        protected virtual void OnCutsceneBegan(CutsceneBegan m)
            => _cutsceneInProgress = true;

        protected virtual void OnCutsceneEnded(CutsceneEnded m)
=> _cutsceneInProgress = false;
    }
}
