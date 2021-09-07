using DavyKager;

using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Game.UI
{
    public class GameWindow : VirtualWindow
    {


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
                [new KeyShortcut(true, false, false, Keys.C)] = CopyCoordinates,
                [new KeyShortcut(Keys.Escape)] = QuitGame,
            };

            RegisterShortcuts(shortcuts);


        }

        private void SayTuttlesPosition() => SayDelegate(World.GetEntity("tuttle").Area.Center.ToString());


        private void MoveTuttleFromClipboard()
            => World.GetEntity("tuttle").ReceiveMessage(new Game.Messaging.Commands.SetPosition(this, new Terrain.Plane(new Vector2(Clipboard.GetText()))));

        private void SayCoordinates() => Tolk.Speak(World.Player.Area.Center.ToString());

        private void CopyCoordinates()
        {
            Clipboard.SetText(World.Player.Area.UpperLeftCorner.ToString());
            SayDelegate("Soiuřadnice zkopírovány.");

        }

        private void QuitGame() =>
            // todo GameWindow.QuitGame
            Environment.Exit(0);









        public override void OnActivate() =>
            //todo GameWindow.OnActivate
            base.OnActivate();

        public override void OnDeactivate() =>
            //todo GameWindow.OnDeActivate

            base.OnDeactivate();

        public override void OnKeyDown(KeyEventParams e)
        {
            base.OnKeyDown(e);
            World.Player.ReceiveMessage(new KeyPressed(this, new KeyShortcut(e)));
        }
    }
}
