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
        /// Indicates if the game loop runs.
        /// </summary>
        private bool _gameLoopEnabled = false;

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
            Text = "Chipotle";
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

            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;
            Shown += MainWindow_Shown;
        }

        /// <summary>
        /// starts or stops the game loop.
        /// </summary>
        public bool GameLoopEnabled
        {
            get => _gameLoopEnabled;
            set => _gameLoopEnabled = _tmrGameLoop.Enabled = value;
        }

        /// <summary>
        /// The game loop
        /// </summary>
        private void GameLoop(object sender, EventArgs e)
            => World.Update();

        /// <summary>
        /// Handler of the KeyDown event
        /// </summary>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
            => WindowHandler.OnKeyDown(new KeyEventParams(e));

        /// <summary>
        /// Handler of the KeyUp event
        /// </summary>
        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
                        => WindowHandler.OnKeyUp(new KeyEventParams(e));

        /// <summary>
        /// Handler of the Shown event
        /// </summary>
        private void MainWindow_Shown(object sender, EventArgs e)
        {
            Focus();
            WindowHandler.MainMenu();
        }
    }
}