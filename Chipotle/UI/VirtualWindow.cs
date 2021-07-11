using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DavyKager;
using Luky;

namespace Game.UI
{
    /// <summary>
    /// Base class for all windows
    /// </summary>
    public abstract class VirtualWindow:DebugSO
    {
        /// <summary>
        /// Construktor
        /// </summary>
        protected VirtualWindow() : this(null)
        {
        }//mtd

        /// <summary>
        /// Adds set of keyboard shortcuts and coresponding actions into _shortcuts dictionary.
        /// </summary>
        /// <param name="shortcuts">Set of shortcuts to be registered</param>
        protected void RegisterShortcuts(Dictionary<KeyShortcut, Action> shortcuts)
        {
            _shortcuts = _shortcuts.Concat(shortcuts).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentWindow">Reference to containing window</param>
        protected VirtualWindow(VirtualWindow parentWindow)
        {
            _parentWindow = parentWindow;
            _shortcuts = new Dictionary<KeyShortcut, Action>();
        }

        /// <summary>
        /// Closes an opened window.
        /// </summary>
        public virtual void Close()
        {
            this.Closed = true;

            // Try to switch to parent window.
            if (_parentWindow !=null)
                WindowHandler.Switch(_parentWindow);
        }



        /// <summary>
        /// KeyDown event handler
        /// </summary>
        /// <param name="e">Event parameters</param>
        public virtual void OnKeyDown(KeyEventParams e)
        {

            Action action = null;
            KeyShortcut tmpShortcut = new KeyShortcut(e);
                        if (_shortcuts!=null && _shortcuts.TryGetValue(tmpShortcut, out action))
                action();
        }

        /// <summary>
        /// OnActivate event handler
        /// </summary>
        public virtual void OnActivate()
        {
            Closed = false;

        }

        /// <summary>
        /// OnClose event handler
        /// </summary>
        public virtual void OnDeactivate()
        {
            Closed = true;
        }

        /// <summary>
        /// Key commands and their handlers
        /// </summary>
        protected Dictionary<KeyShortcut, Action> _shortcuts;

        /// <summary>
        /// Reference to containing window
        /// </summary>
        protected VirtualWindow _parentWindow;


        /// <summary>
        /// Indicates if the window is closed
        /// </summary>
        public bool Closed { get; private set; }
    }
}
