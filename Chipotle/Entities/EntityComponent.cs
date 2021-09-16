using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Events;

using Luky;

namespace Game.Entities
{
    public abstract class EntityComponent : MessagingObject
    {
        public Entity Owner;
        protected bool _cutsceneInProgress;

        public Name Name => Owner?.Name;

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
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