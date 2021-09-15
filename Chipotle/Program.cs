using System;
using System.Windows.Forms;

using DavyKager;

using Game.UI;

using Luky;

namespace Game
{
    internal class Program : DebugSO
    {
        public static readonly string DataPath = @"Data\";

        private const bool _abort = false;

        public static MainWindow MainWindow { get; private set; }

        public static void OnError(Exception ex)
        => OnError(ex.ToString());

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
   => OnError(e.Exception);

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
  => OnError(e.ExceptionObject.ToString());

        [STAThread]
        private static void Main()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                Tolk.Load();
                Tolk.TrySAPI(true);
                World.SoundInit((message) => Tolk.Speak(message));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            MainWindow = new MainWindow();
            Application.Run(MainWindow);
        }

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