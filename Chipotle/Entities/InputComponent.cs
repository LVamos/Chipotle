using Game.Messaging;
using Game.Messaging.Events;
using Game.UI;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Entities
{
    public abstract class InputComponent : EntityComponent
    {



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
        /// Responds on a keypress.
        /// </summary>
        /// <param name="shortcut"></param>
        protected virtual void OnKeyDown(KeyPressed message)
        {
            if (_shortcuts != null && _shortcuts.TryGetValue(message.Shortcut, out Action action))
            {
                action();
            }
        }


        /// <summary>
        /// Adds set of keyboard shortcuts and coresponding actions into _shortcuts dictionary.
        /// </summary>
        /// <param name="shortcuts">Set of shortcuts to be registered</param>
        protected void RegisterShortcuts(Dictionary<KeyShortcut, Action> shortcuts) => _shortcuts = _shortcuts.Concat(shortcuts).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);


        protected Dictionary<KeyShortcut, Action> _shortcuts;
    }
}
