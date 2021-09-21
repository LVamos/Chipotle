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
    public class GameWindow : VirtualWindow
    {
        /// <summary>
        /// constructor
        /// </summary>
        public GameWindow()
        {
            Dictionary<KeyShortcut, Action> shortcuts = new Dictionary<KeyShortcut, Action>()
            {
                [new KeyShortcut(Keys.C)] = SayCoordinates,
                [new KeyShortcut(Keys.Escape)] = QuitGame,
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
            World.Player.ReceiveMessage(new KeyPressed(this, new KeyShortcut(e)));
        }

        /// <summary>
        /// Quits the game.
        /// </summary>
        private void QuitGame()
            => World.QuitGame();

        /// <summary>
        /// Reports relative coordiantes of the Detective Chipotle NPC.
        /// </summary>
        private void SayCoordinates()
            => Tolk.Speak(World.Player.Area.ToRelative().Center.ToString());
    }
}