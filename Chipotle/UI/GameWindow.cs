using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DavyKager;

using Game.Messaging.Events;

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
            Dictionary<KeyShortcut, Action> shortcuts = new Dictionary<KeyShortcut, Action>()
            {
                [new KeyShortcut(Keys.Escape)] = QuitGame
            };

            RegisterShortcuts(shortcuts);
        }

        /// <summary>
        /// Processes the KeyDown message.
        /// </summary>
        /// <param name="e">The message to be handled</param>
        public override void OnKeyDown(KeyEventParams e)
        {
            base.OnKeyDown(e);

            if (Program.MainWindow.GameLoopEnabled)
                World.Player.ReceiveMessage(new KeyPressed(this, new KeyShortcut(e)));
        }

        /// <summary>
        /// Processes the KeyUpmessage.
        /// </summary>
        /// <param name="e">The message to be handled</param>
        public override void OnKeyUp(KeyEventParams e)
        {
            if (Program.MainWindow.GameLoopEnabled && World.Player!=null)
                World.Player.ReceiveMessage(new KeyReleased(this, new KeyShortcut(e)));
        }

        /// <summary>
        /// Quits the game.
        /// </summary>
        private void QuitGame()
            => World.QuitGame();
    }
}