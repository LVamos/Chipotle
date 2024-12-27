using Game.Messaging.Events;

using System;
using System.Windows.Forms;

namespace Game.UI
{
    /// <summary>
    /// A virtual window opened during the game
    /// </summary>
    [Serializable]
    public class GameWindow : VirtualWindow
    {
        /// <summary>
        /// constructor
        /// </summary>
        public GameWindow()
        {
            RegisterShortcuts(
                (new KeyShortcut(Keys.Escape), QuitGame),
                (new KeyShortcut(KeyShortcut.Modifiers.Control, Keys.Z), Program.SendFeedback)
                );
        }

        /// <summary>
        /// Processes the KeyDown message.
        /// </summary>
        /// <param name="e">The message to be handled</param>
        public override void OnKeyDown(KeyEventParams e)
        {
            base.OnKeyDown(e);

            if (Program.MainWindow.GameInProgress)
                World.Player.TakeMessage(new KeyPressed(this, new KeyShortcut(e)));
        }

        /// <summary>
        /// Processes the KeyUpmessage.
        /// </summary>
        /// <param name="e">The message to be handled</param>
        public override void OnKeyUp(KeyEventParams e)
        {
            if (Program.MainWindow.GameInProgress && World.Player != null)
                World.Player.TakeMessage(new KeyReleased(this, new KeyShortcut(e)));
        }

        /// <summary>
        /// Quits the game.
        /// </summary>
        private void QuitGame()
            => World.QuitGame();
    }
}