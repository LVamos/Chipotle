using System;

using DavyKager;

namespace Game.UI
{
    /// <summary>
    /// Represents a virtual window opened during the main menu.
    /// </summary>
    public class MainMenuWindow : VirtualWindow
    {
        /// <summary>
        /// Actions performed when the window is activated.
        /// </summary>
        public override void OnActivate()
        {
            base.OnActivate();
            RunMainMenu();
        }

        /// <summary>
        /// Runs the main menu.
        /// </summary>
        public void RunMainMenu()
        {
            string[] items = new string[]
            {
                "Nová hra",
                "Test sluchátek",
                "Návod",
                "Konec"
            };
            string intro = "Hlavní menu";
            int choice = WindowHandler.Menu(items, intro, true);
            switch (choice)
            {
                case 0: WindowHandler.StartGame(); break;
                case 1: SpeakerTest(); RunMainMenu(); break;
                case 2: Help(); RunMainMenu(); break;
                default: ExitGame(); break;
            }
        }

        /// <summary>
        /// Plays the speaker test.
        /// </summary>
        public void SpeakerTest()
                            => World.Sound.Play("SpeakerTest");

        /// <summary>
        /// closes the application.
        /// </summary>
        private void ExitGame()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Opens help.
        /// </summary>
        private void Help()
        {
            Tolk.Speak("Otvírám nápovědu");
            System.Diagnostics.Process.Start("help.html");
        }
    }
}