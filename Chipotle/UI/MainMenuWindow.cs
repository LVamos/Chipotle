using DavyKager;
using System.Threading;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Game.UI
{
    public class MainMenuWindow: VirtualWindow
    {
        public void SpeakerTest()
            => World.Sound.Play("SpeakerTest");

        private void Help()
        {
            SayDelegate("Otvírám nápovědu");
            System.Diagnostics.Process.Start("help.html");
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
                    case 1: SpeakerTest(); RunMainMenu();  break;
                    case 2: Help(); RunMainMenu();  break;
                    default: ExitGame(); break;
                }
        }

        private void ExitGame()
        {
            Environment.Exit(0);
        }


        public override void OnActivate()
        {
            base.OnActivate();
            RunMainMenu();
        }

    }
}
