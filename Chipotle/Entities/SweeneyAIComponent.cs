using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Controls behavior of the Derreck Sweeney NPC.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class SweeneyAIComponent : AIComponent
    {
        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();
            InnerMessage(new SetPosition(this, new Rectangle("1402, 960"), true));
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
                case "cs23": JumpToSweeneysRoom(); break;
            }
        }

        /// <summary>
        /// Relocates the Detective Chipotle and Tuttle NPCs from Easterby street (ulice s1)
        /// locality to the sweeney's hall (hala s1) locality.
        /// </summary>
        private void JumpToSweeneysRoom()
            => InnerMessage(new SetPosition(this, new Rectangle("1407, 978"), true));
    }
}