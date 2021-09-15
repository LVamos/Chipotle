using System;

using DavyKager;

namespace Game.UI
{
    public class MainMenuWindow : VirtualWindow
    {
        public override void OnActivate()
        {
            base.OnActivate();
            RunMainMenu();
        }

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

        public void SpeakerTest()
                            => World.Sound.Play("SpeakerTest");

        private void ExitGame()
        {
            Environment.Exit(0);
        }

        private void Help()
        {
            Tolk.Speak("Otvírám nápovědu");
            System.Diagnostics.Process.Start("help.html");
        }
    }
}