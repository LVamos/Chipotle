using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Events;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Base class for all NPC components
    /// </summary>
    [Serializable]
    public abstract class EntityComponent : MessagingObject
    {
        /// <summary>
        /// A reference to the parent NPC
        /// </summary>
        public Entity Owner;

        /// <summary>
        /// Indicates if a cutscene is played in the moment.
        /// </summary>
        protected bool _cutsceneInProgress;

        /// <summary>
        /// Name of the parent NPC
        /// </summary>
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

        /// <summary>
        /// Processes the CutsceneBegan message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnCutsceneBegan(CutsceneBegan message)
            => _cutsceneInProgress = true;

        /// <summary>
        /// Processes the CutsceneEnded message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnCutsceneEnded(CutsceneEnded message)
=> _cutsceneInProgress = false;
    }
}