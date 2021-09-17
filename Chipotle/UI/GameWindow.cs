using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DavyKager;

using Game.Entities;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

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
                // test shortcuts for moving entities and objects
                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.T)] = SayTuttlesPosition,

                [new KeyShortcut(KeyShortcut.Modifiers.Shift, Keys.J)] = MoveTuttleFromClipboard,

                // Test shortcuts for reverb settings
                [new KeyShortcut(Keys.PageDown)] = () => World.Sound.SwitchToNextReverbPreset(),
                [new KeyShortcut(Keys.F1)] = () => World.Sound.PreviousReverbParameter(),
                [new KeyShortcut(Keys.F2)] = () => World.Sound.NextReverbParameter(),
                [new KeyShortcut(Keys.F3)] = () => World.Sound.DecreaseReverbParameter(),
                [new KeyShortcut(Keys.F4)] = () => World.Sound.IncreaseReverbParameter(),
                [new KeyShortcut(Keys.F5)] = () => World.Sound.SetReverbParameterToDefault(),
                [new KeyShortcut(Keys.F6)] = () => World.Sound.SetReverbParameterToMinimum(),
                [new KeyShortcut(Keys.F7)] = () => World.Sound.SetReverbParameterToMaximum(),
                [new KeyShortcut(Keys.F8)] = () => World.Sound.SayReverbParameterValue(),

                // Other shortcuts
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
        /// A test method to move the Detective Chipotle NPC to coordinates optained from clipboard.
        /// </summary>
        private void MoveTuttleFromClipboard()
        {
            if (!_testModeEnabled)
                return;

            Entity tuttle = World.GetEntity("tuttle");
            Vector2 position = Clipboard.GetText().ToVector2();
            SetPosition message = new SetPosition(this, new Plane(position));
            tuttle.ReceiveMessage(message);
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


        private void SayTuttlesPosition()
        {
            if(_testModeEnabled)
                 Tolk.Speak(World.GetEntity("tuttle").Area.Center.ToString());
        }
    }
}