using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Events;
using Game.UI;

namespace Game.Entities
{
    /// <summary>
    /// Allows the player to control an NPC.
    /// </summary>
    [Serializable]
    public abstract class InputComponent : EntityComponent
    {
        /// <summary>
        /// Registered keyboard shortcuts and corresponding actions
        /// </summary>
        protected Dictionary<KeyShortcut, Action> _shortcuts;

        /// <summary>
        /// Constructor
        /// </summary>
        public InputComponent() : base()
        {
            // Assign an KeyDown handler
            Dictionary<Type, Action<GameMessage>> handlers = new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(KeyPressed)] = (m) => OnKeyDown((KeyPressed)m),
                [typeof(CutsceneBegan)] = (m) => OnCutsceneBegan((CutsceneBegan)m),
                [typeof(CutsceneEnded)] = (m) => OnCutsceneEnded((CutsceneEnded)m)
            };
            RegisterMessages(handlers);
            _shortcuts = new Dictionary<KeyShortcut, Action>();
        }

        /// <summary>
        /// Processes the KeyDown message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnKeyDown(KeyPressed message)
        {
            if (_shortcuts != null && _shortcuts.TryGetValue(message.Shortcut, out Action action))
                action();
        }

        /// <summary>
        /// Registers keyboard shortcuts and corresponding actions.
        /// </summary>
        /// <param name="shortcuts">Set of shortcuts to be registered</param>
        protected void RegisterShortcuts(Dictionary<KeyShortcut, Action> shortcuts) => _shortcuts = _shortcuts.Concat(shortcuts).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
    }
}