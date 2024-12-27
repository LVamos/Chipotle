using Game.Messaging;
using Game.Messaging.Events;
using Game.UI;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Entities
{
    /// <summary>
    /// Allows the player to control an NPC.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(ChipotleInputComponent))]
    public abstract class InputComponent : CharacterComponent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public InputComponent()
            => RegisterShortcuts();

        /// <summary>
        /// Registered keyboard shortcuts and corresponding actions
        /// </summary>
        [ProtoIgnore]
        protected Dictionary<KeyShortcut, Action> _shortcuts;

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case KeyPressed kp: OnKeyDown(kp); break;
                case CutsceneEnded ce: OnCutsceneEnded(ce); break;
                case CutsceneBegan cb: OnCutsceneBegan(cb); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Processes the KeyDown message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnKeyDown(KeyPressed message)
        {
            if (_shortcuts == null)
                RegisterShortcuts();

            if (_shortcuts != null && _shortcuts.TryGetValue(message.Shortcut, out Action action))
                action();
        }

        /// <summary>
        /// registers keyboard shotctus for the component.
        /// </summary>
        protected virtual void RegisterShortcuts()
        {
            if (_shortcuts == null)
                _shortcuts = new Dictionary<KeyShortcut, Action>();
        }

        /// <summary>
        /// Registers keyboard shortcuts and corresponding actions.
        /// </summary>
        /// <param name="shortcuts">Set of shortcuts to be registered</param>
        protected void AddShortcuts(Dictionary<KeyShortcut, Action> shortcuts) => _shortcuts = _shortcuts.Concat(shortcuts).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
    }
}