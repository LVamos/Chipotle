using Game.Messaging;
using Game.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Game.UI;

namespace Game.Entities
{
	public abstract  class InputComponent: EntityComponent
    {



        public InputComponent():base()
        {
            // Assign an KeyDown handler
            var handlers = new Dictionary<Type, Action<GameMessage>>()
            {
                [typeof(KeyPressed )] =(m)=>  OnKeyDown((KeyPressed )m),
                [typeof(CutsceneBegan)] =(m)=>  OnCutSceneStart((CutsceneBegan)m),
                [typeof(CutsceneEnded)] =(m)=>  OnCutSceneEnd((CutsceneEnded)m)
            };
            RegisterMessageHandlers(handlers);
            _shortcuts = new Dictionary<KeyShortcut, Action>();
        }

        private void OnCutSceneEnd(GameMessage message)
=> _messagingEnabled = false;

        private void OnCutSceneStart(CutsceneBegan message)
=> _messagingEnabled = true;



        /// <summary>
        /// Responds on a keypress.
        /// </summary>
        /// <param name="shortcut"></param>
        protected void OnKeyDown(KeyPressed  message)
        {
            if (_shortcuts != null && _shortcuts.TryGetValue(message.Shortcut, out var action))
                action();
        }


        /// <summary>
        /// Adds set of keyboard shortcuts and coresponding actions into _shortcuts dictionary.
        /// </summary>
        /// <param name="shortcuts">Set of shortcuts to be registered</param>
        protected void RegisterShortcuts(Dictionary<KeyShortcut, Action> shortcuts)
        {
            _shortcuts = _shortcuts.Concat(shortcuts).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
        }



        protected Dictionary<KeyShortcut, Action> _shortcuts;

    }
}
