using System.Threading.Tasks;
using System;
using System.IO;

using DavyKager;

using Luky;
using OpenTK;
using System.Threading;
using System.Windows.Forms;

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
        /// Name of the background sound loop
        /// </summary>
        private const string _menuLoopSound = "MainMenuLoop";

        /// <summary>
        /// A handle of the background sound loop
        /// </summary>
        private int _menuLoopID;

        /// <summary>
        /// Handle of the sound used for speaker test
        /// </summary>
        private int _speakerTestSoundID;

        /// <summary>
        /// Specifies if the loop should be replayed when user returns to the menu.
        /// </summary>
        private bool _loopInProgress = false;

        /// <summary>
        /// Name of the sound with the speaker test.
        /// </summary>
        private const string _speakerTestSound= "SpeakerTest";

        /// <summary>
        /// Name of the sound played when the application is being closed
        /// </summary>
        private const string _endSound = "MainMenuLongEnd";

        /// <summary>
        /// Name of the menu loop end sound
        /// </summary>
        private const string _shortEndSound = "MainMenuLongEnd";

        /// <summary>
        /// Runs the main menu.
        /// </summary>
        public void RunMainMenu()
        {
            string intro =_menuLoopID == 0 ? "Hlavní menu" : String.Empty;
            PlayLoop();

            string[] items;

            if (File.Exists(Path.Combine(Program.DataPath, "game.sav")))
            {
                items = new string[]
            {
                "Nová hra",
                "Pokračovat ve hře",
                "Test sluchátek",
                "Návod",
                "Konec"
            };
            }
            else
            {
                items = new string[]
                {
                "Nová hra",
                "Test sluchátek",
                "Návod",
                "Konec"
                };
            }

            int choice = WindowHandler.Menu(items, intro, true);
            choice = choice == -1 ? items.Length - 1 : choice;
            switch (items[choice])
            {
                case "Nová hra": StartGame(); break;
                case "Pokračovat ve hře": LoadGame(); break;
                case "Test sluchátek": SpeakerTest(); break;
                case "Návod": Help(); RunMainMenu(); break;
                default: ExitGame(); break;
            }
        }

        /// <summary>
        /// Loads a saved game from file.
        /// </summary>
        private void LoadGame()
        {
            StopLoop();
            World.LoadGame();
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        private void StartGame()
        {
            StopLoop(); 
            WindowHandler.StartGame();
        }

        /// <summary>
        /// Fades the menu loop sound and plays a closing jingle.
        /// </summary>
        /// <param name="shortEnd">Specifies if the closing jingle should be short or long.</param>
        private void StopLoop()
        {
            World.Sound.FadeSourceOut(_menuLoopID, .1f);
            World.Sound.Play(World.Sound.GetSoundStream(_endSound), null, false, PositionType.None, Vector3.Zero, false, .3f, null, 1, 0, Playback.OpenAL);
        }

        /// <summary>
        /// Starts the menu loop sound if the menu is newly created.
        /// </summary>
        private void PlayLoop()
        {
            if (_menuLoopID == 0)
                _menuLoopID = World.Sound.Play(_menuLoopSound, null, true, PositionType.None, Vector3.Zero, false, .3f);
        }

        /// <summary>
        /// Plays the speaker test.
        /// </summary>
        public void SpeakerTest()
        {
            Task.Run(() => {
                World.Sound.FadeSource(_menuLoopID, FadingType.Out, .0001f, .1f, false);
                _speakerTestSoundID = World.Sound.Play(_speakerTestSound);
                Thread.Sleep(3600);
                World.Sound.FadeSource(_menuLoopID, FadingType.In, .0001f, .3f); Thread.Sleep(4000);
            });
            _loopInProgress = true;
            RunMainMenu();
        }

        /// <summary>
        /// closes the application.
        /// </summary>
        private void ExitGame()
        {
            StopLoop();
            Thread.Sleep(7000);
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

        /// <summary>
        /// Handles the KeyDown message.
        /// </summary>
        /// <param name="e">The message</param>
        public override void OnKeyDown(KeyEventParams e)
        {
            World.Sound.GetDynamicInfo(_speakerTestSoundID, out SoundState s, out int _);
            if (s == SoundState.Playing)
                World.Sound.Stop(_speakerTestSoundID);

             base.OnKeyDown(e);
        }

    }
}