using System;
using System.Collections.Generic;
using System.Linq;

using Luky;

namespace Game.UI
{
    /// <summary>
    /// Base class for all virtual windows
    /// </summary>
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
        /// KeyUp event handler
        /// </summary>
        /// <param name="e">Event parameters</param>
        /// <remarks>Must be implemented in descendants.</remarks>
        public virtual void OnKeyUp(KeyEventParams e)
        { }

        /// <summary>
        /// Registers shotcuts and corresponding actions.
        /// </summary>
        /// <param name="shortcuts">Set of shortcuts to be registered</param>
        protected void RegisterShortcuts(Dictionary<KeyShortcut, Action> shortcuts) => _shortcuts = _shortcuts.Concat(shortcuts).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
    }
}