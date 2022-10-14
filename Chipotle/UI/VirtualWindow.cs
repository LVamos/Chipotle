using Luky;

using System;
using System.Collections.Generic;

namespace Game.UI
{
    /// <summary>
    /// Base class for all virtual windows
    /// </summary>
    [Serializable]
    public abstract class VirtualWindow : DebugSO
    {
        /// <summary>
        /// Reference to parent window
        /// </summary>
        protected VirtualWindow _parentWindow;

        /// <summary>
        /// Key commands and their handlers
        /// </summary>
        protected Dictionary<KeyShortcut, Action> _shortcuts;

        /// <summary>
        /// Construktor
        /// </summary>
        protected VirtualWindow() : this(null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentWindow">Reference to parent window</param>
        protected VirtualWindow(VirtualWindow parentWindow)
        {
            _parentWindow = parentWindow;
            _shortcuts = new Dictionary<KeyShortcut, Action>();
        }

        /// <summary>
        /// Indicates if the window is closed
        /// </summary>
        public bool Closed { get; private set; }

        /// <summary>
        /// Closes an opened window.
        /// </summary>
        public virtual void Close()
        {
            Closed = true;

            // Try to switch to parent window.
            if (_parentWindow != null)
                WindowHandler.Switch(_parentWindow);
        }

        /// <summary>
        /// OnActivate event handler
        /// </summary>
        public virtual void OnActivate() => Closed = false;

        /// <summary>
        /// OnClose event handler
        /// </summary>
        public virtual void OnDeactivate() => Closed = true;

        /// <summary>
        /// KeyDown event handler
        /// </summary>
        /// <param name="e">Event parameters</param>
        public virtual void OnKeyDown(KeyEventParams e)
        {
            KeyShortcut tmpShortcut = new KeyShortcut(e);
            if (_shortcuts != null && _shortcuts.TryGetValue(tmpShortcut, out Action action))
                action();
        }

        /// <summary>
        /// Handles the KeyPress message.
        /// </summary>
        /// <param name="letter">The key that was pressed</param>
        public virtual void OnKeyPress(char letter) { }

        /// <summary>
        /// KeyUp event handler
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <remarks>Must be implemented in descendants.</remarks>
        public virtual void OnKeyUp(KeyEventParams e)
        { }

        /// <summary>
        /// Registers shotcuts and corresponding actions.
        /// </summary>
        /// <remarks>If a shortcut is already registered it'll be overriden.</remarks>
        /// <param name="shortcuts">Set of shortcuts to be registered</param>
        protected void RegisterShortcuts(params (KeyShortcut shortcut, Action action)[] shortcuts)
        {
            foreach ((KeyShortcut shortcut, Action action) shortcut in shortcuts)
                _shortcuts[shortcut.shortcut] = shortcut.action;
        }
    }
}