using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using DavyKager;

using Game.UI;

using Luky;

using OpenTK;

namespace Game
{
    /// <summary>
    /// Entry point of the application
    /// </summary>
    internal class Program : DebugSO
    {
        /// <summary>
        /// Enables some test methods.
        /// </summary>
        public const bool TestMode = true;


        /// <summary>
        /// Path to data folder
        /// </summary>
        public static readonly string DataPath = @"Data\";

        public static readonly Action<string> TolkDelegate = (message) => Tolk.Speak(message);

        /// <summary>
        /// Reference to the main window
        /// </summary>
        public static MainWindow MainWindow { get; private set; }

        /// <summary>
        /// Path to file used for serialization.
        /// </summary>
        public static string SerializationPath { get; private set; }

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="ex">The exception</param>
        public static void OnError(Exception ex)
        {
            throw ex;
        OnError(ex.ToString());
        }

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="sender">Source of the exception</param>
        /// <param name="e">The exception</param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            throw e.Exception;
   OnError(e.Exception);
        }

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="sender">Source of the exception</param>
        /// <param name="e">The exception</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw (Exception)e.ExceptionObject;
            OnError(e.ExceptionObject.ToString());
        }

        /// <summary>
        /// entry point of the application
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            SerializationPath = Path.Combine(DataPath, "game.sav");

            try
            {
                Tolk.Load();
                Tolk.TrySAPI(true);
                DisableJAWSKeyHook();
                World.SoundInit(TolkDelegate);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            MainWindow = new MainWindow();
            Application.Run(MainWindow);
            EnableJAWSKeyHook();
        }

        /// <summary>
        /// Stores reference to JAWS COM object.
        /// </summary>
        private static Type _jaws;

        private static Object _jawsObject;

        /// <summary>
        /// Sets access to JAWS COM object.
        /// </summary>
        /// <returns>True if JAWS could be initialized</returns>
        private static bool InitJAWS()
        {
            if (_jaws != null)
                return true;

                _jaws = Type.GetTypeFromProgID("FreedomSci.JawsApi");

                if (_jaws == null)
                    return false;

            _jawsObject = Activator.CreateInstance(_jaws);
            return true;
        }

        /// <summary>
        /// Enables JAWS keyhook.
        /// </summary>
        public static void EnableJAWSKeyHook()
        {
            if (!InitJAWS())
                return;

            _jaws.InvokeMember("Enable", System.Reflection.BindingFlags.InvokeMethod, null, _jawsObject, new object[] { true });
        }

        /// <summary>
        /// Removes JAWS keyhook.
        /// </summary>
        public static void DisableJAWSKeyHook()
        {
            if (!InitJAWS())
return;

            _jaws.InvokeMember("Disable", System.Reflection.BindingFlags.InvokeMethod, null, _jawsObject, null);
        }

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="error">The error message</param>
        private static void OnError(string error)
        {
            EnableJAWSKeyHook();
            string text = string.Empty;

            if (World.Player != null)
                text = $"Current position: {World.Player.Area.Center.ToString()}{Environment.NewLine}";
            text += error;
            Action action = () =>
            {
                MessageBox.Show(MainWindow, $"Hlášení o chybě zkopírováno do schránky{Environment.NewLine}" + text, AppDomain.CurrentDomain.FriendlyName + ": závažná chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Clipboard.SetText(text);
            };

            if (MainWindow != null)
                MainWindow.Invoke(action);
            else
                action();

            Environment.Exit(0);
        }
    }
}