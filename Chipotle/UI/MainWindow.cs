using DavyKager;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Game.UI
{
    /// <summary>
    /// The only visible window which catches keyboard events.
    /// </summary>
    public class MainWindow : Form
    {
        /// <summary>
        /// Runs the game loop.
        /// </summary>
        private readonly Timer _tmrGameLoop;

        /// <summary>
        /// Indicates if the master volume is muted.
        /// </summary>
        private bool _muted;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            ControlBox = false;
            Cursor = Cursors.Default;
            Name = "MainWindow";
            ResumeLayout(false);
            Text = "Chipotle " + Program.Version;
            ShowIcon = false;
            MaximizeBox = false;
            MinimizeBox = false;
            WindowState = FormWindowState.Maximized;
            AutoSize = true;
            ShowInTaskbar = false;
            KeyPreview = true;

            _tmrGameLoop = new Timer
            {
                Interval = 1000 / World.FramesPerSecond
            };
            _tmrGameLoop.Tick += GameLoop;
            _tmrGameLoop.Enabled = false;

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            KeyPress += OnKeyPress;
            Shown += OnShown;
            FormClosing += OnFormClosing;
            Deactivate += OnDeactivate;
            Activated += OnActivated;
        }

        /// <summary>
        /// Handles the KeyPress message.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="e">The message</param>
        private void OnKeyPress(object sender, KeyPressEventArgs e)
            => WindowHandler.OnKeyPress(e.KeyChar);

        /// <summary>
        /// Handles the Activated message.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="e">The message</param>
        private void OnActivated(object sender, EventArgs e)
        {
            Program.DisableJAWSKeyHook();

            if (World.Sound.Muted)
            {
                World.ResumeCutscene();
                World.Sound.Unmute();
            }
        }

        /// <summary>
        /// Handles the Deactivate message.
        /// </summary>
        /// <param name="sender">Source fo the message</param>
        /// <param name="e">The message</param>
        private void OnDeactivate(object sender, EventArgs e)
        {
            Program.EnableJAWSKeyHook();
            World.PauseCutscene();
            World.Sound.Mute();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            if (GameInProgress)
                World.SaveGame();

            World.Sound.StopAll();
            Program.EnableJAWSKeyHook();
            Environment.Exit(0);
        }

        /// <summary>
        /// starts or stops the game loop.
        /// </summary>
        public bool GameInProgress
        {
            get => _tmrGameLoop != null ? _tmrGameLoop.Enabled : false;
            set => _tmrGameLoop.Enabled = value;
        }

        /// <summary>
        /// The game loop
        /// </summary>
        private void GameLoop(object sender, EventArgs e)
            => World.Update();

        /// <summary>
        /// Handler of the KeyDown event
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            InterruptSpeech();
            WindowHandler.OnKeyDown(new KeyEventParams(e));
        }

        /// <summary>
        /// Interrupts an ongoing SAPI or screen reader utterance.
        /// </summary>
        /// <remarks>Works with SAPI and JAWS only. NVDA does this automatically.</remarks>
        private void InterruptSpeech()
        {
            Tolk.Silence();

            if (Tolk.DetectScreenReader() == "JAWS")
                Tolk.Speak(String.Empty, true);
        }

        /// <summary>
        /// Handler of the KeyUp event
        /// </summary>
        private void OnKeyUp(object sender, KeyEventArgs e)
                        => WindowHandler.OnKeyUp(new KeyEventParams(e));

        /// <summary>
        /// Handler of the Shown event
        /// </summary>
        private void OnShown(object sender, EventArgs e)
        {
            Activate();
            WindowState = FormWindowState.Maximized;
            Focus();

            if (!Program.Settings.MainMenuAtStartup)
                WindowHandler.StartGame();
            else
                WindowHandler.MainMenu();

        }
    }
}