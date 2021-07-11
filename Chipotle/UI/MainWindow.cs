using Luky;
using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Game.UI;

namespace Game.UI
{
    /// <summary>
    /// The only visible window which catches keyboard events.
    /// </summary>
  public  class MainWindow: Form
    {
        private Timer _tmrGameLoop;
        private bool _gameLoopEnabled=false;
        public const int FramesPerSecond = 1000 / 66;
        public bool GameLoopEnabled
        {
            get => _gameLoopEnabled;
            set => _gameLoopEnabled = _tmrGameLoop.Enabled = value;
        }

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


            _tmrGameLoop = new Timer();
            _tmrGameLoop.Interval = FramesPerSecond;
            _tmrGameLoop.Tick += GameLoop;
            _tmrGameLoop.Enabled = false;

            KeyDown += MainWindow_KeyDown;
            Shown += MainWindow_Shown;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            World.Update();
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            Focus();
            WindowHandler.Switch(new GameWindow());
            World.StartGame();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
            => WindowHandler.OnKeyDown(new KeyEventParams(e));




    }
}
