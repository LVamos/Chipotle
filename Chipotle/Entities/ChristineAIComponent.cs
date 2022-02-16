using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

namespace Game.Entities
{
    /// <summary>
    /// Controls behavior of the Christine Pierce NPC
    /// </summary>
    [Serializable]
    public class ChristineAIComponent : AIComponent
    {
        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Start()
        {
            base.Start();
            Owner.ReceiveMessage(new SetPosition(this, new Plane("1792, 1127"), true));
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case CutsceneEnded ce: OnCutsceneEnded(ce); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Processes the CutsceneEnded message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnCutsceneEnded(CutsceneEnded message)
        {
            base.OnCutsceneEnded(message);

            switch (message.CutsceneName)
            {
                case "cs21": JumpToBedroom(); break;
            }
        }

        /// <summary>
        /// Moves the NPC to the Christine's bed room (ložnice p1) locality.
        /// </summary>
        private void JumpToBedroom()
            => Owner.ReceiveMessage(new SetPosition(this, new Plane("1744, 1123"), true));
    }
}