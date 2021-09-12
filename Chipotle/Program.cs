using System.Reflection;
using DavyKager;

using Game.UI;

using Luky;

using System;
using System.IO;
using System.Windows.Forms;

namespace Game
{
    internal class Program : DebugSO
    {
        private const bool _abort = false;

        public static MainWindow MainWindow { get; private set; }

        [STAThread]
        private static void Main()
        {




            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            DataPath = @"Data\";
            SoundAssetsPath = Path.Combine(DataPath, "Sounds");

            try
            {
                Tolk.Load();
                Tolk.TrySAPI(true);
                SayDelegate = (message) => Tolk.Speak(message);
            World.SoundInit();
                MainWindow = new MainWindow();
                Application.Run(MainWindow);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
  => OnError(e.ExceptionObject.ToString());

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
   => OnError(e.Exception);

        public static void OnError(Exception ex)
        => OnError(ex.ToString());

        private static void OnError(string error)
        {
            Action action = () => MessageBox.Show(MainWindow, error, AppDomain.CurrentDomain.FriendlyName + ": závažná chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (MainWindow != null)
                MainWindow.Invoke(action);
            else
                action();

            Environment.Exit(0);
        }
    }
}
