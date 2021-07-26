using System.Threading;
using System.Windows.Forms;

namespace Game.UI
{
    /// <summary>
    /// Manages user interface, redistributes keyboard input to windows
    /// </summary>
    public static class WindowHandler
    {
        /// <summary>
        /// Currently focused window
        /// </summary>
        public static VirtualWindow ActiveWindow { get => _activeWindow; private set => _activeWindow = value; }

        /// <summary>
        /// Reference to previously active window
        /// </summary>
        public static VirtualWindow PreviousWindow { get; private set; }


        /// <summary>
        /// Closes currently active window and activates another one.
        /// </summary>
        /// <param name="window">New window</param>
        public static void Switch(VirtualWindow window)
        {
            _activeWindow?.OnDeactivate(); // Let active window react on deactivating
            PreviousWindow = ActiveWindow; // Backing up for future use
            _activeWindow = window;
            ActiveWindow.OnActivate(); // Let new window react on activation
        }

        /// <summary>
        /// Opens virtual modal window
        /// </summary>
        /// <param name="modalWindow">Reference to new window</param>
        public static void OpenModalWindow(VirtualWindow modalWindow)
        {
            PreviousWindow = ActiveWindow;
            _activeWindow = modalWindow;
            _activeWindow.OnActivate();
            while (!ActiveWindow.Closed)
            {
                Thread.Sleep(5);
                Application.DoEvents();
            }//wh
            _activeWindow = PreviousWindow;
            {
            }
        }



        /// <summary>
        /// Static constructor
        /// </summary>
        static WindowHandler() => _activeWindow = null;

        /// <summary>
        /// Internal reference to currently active window
        /// </summary>
        private static VirtualWindow _activeWindow;

        /// <summary>
        /// Delegates event to event handler of active window
        /// </summary>
        /// <param name="e">Event parameters</param>
        public static void OnKeyDown(KeyEventParams e)
           => ActiveWindow?.OnKeyDown(e);
    }
}
