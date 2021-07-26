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



            //PreJit.ForceLoadAll(Assembly.GetCallingAssembly());
            //PreJit.PreJITMethods(Assembly.GetCallingAssembly());

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                Tolk.Load();
                SayDelegate = (message) => Tolk.Speak(message);
                DataPath = @"Data\";
                SoundAssetsPath = Path.Combine(DataPath, "Sounds");
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
            {
                MainWindow.Invoke(action);
            }
            else
            {
                action();
            }

            Environment.Exit(0);
        }
    }
}
