using System.Net;
using System.Net.Mail;
using System;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

using DavyKager;

using Game.UI;

using Luky;

using OpenTK;
using System.Text;

namespace Game
{
    /// <summary>
    /// Entry point of the application
    /// </summary>
    internal class Program : DebugSO
    {
        /// <summary>
        /// Current version of the game
        /// </summary>
        public static string Version = "0.4.2";

        /// <summary>
        /// Sends an e-mail message to my Gmail account.
        /// </summary>
        /// <param name="subject">Subject for the message</param>
        /// <param name="body">Body of the message</param>
        /// <returns>True if the message is sent properly</returns>
        private static bool SendMeMail(string subject, string body, Attachment attachment)
        {
            var fromAddress = new MailAddress("lukas.vamos@gmail.com", Environment.UserName);
            var toAddress = new MailAddress("lukas.vamos@gmail.com", "Lukáš Vámoš");
            const string fromPassword = "zphoeiuuqlbvemgy";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            // Send it

            try
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    if (attachment != null)
                        message.Attachments.Add(attachment);

                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static bool ReportError(string message)
        {
            string path = @"data\game.sav";
            Attachment attachment = File.Exists(path) ? new Attachment(path) : null;
            return SendMeMail("Chipotle - hlášení o chybě", message, attachment);
        }

        /// <summary>
        /// Enables some test methods.
        /// </summary>
        public const bool TestMode = false;

        /// <summary>
        /// Path to data folder
        /// </summary>
        public static readonly string DataPath = @"Data\";

        public static readonly Action<string> TolkDelegate = (message) => Tolk.Speak(message, true);

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
        public static void OnError(Exception ex, string message = null)
        {
                EnableJAWSKeyHook(); // Restore JAWS key hook

            // Prepare an error message
                StringBuilder text = new StringBuilder();
                text.AppendLine("Hlášení o pádu Chipotla.");
                text.AppendLine("Uživatel: " + Environment.UserName);
                text.AppendLine("Verze: " + Version);
                text.AppendLine("Datum a čas: " + DateTime.Now.ToString());

                if (World.Player != null)
                    text.AppendLine("Aktuální pozice Chipotla: " + World.Player.Area.Center.ToString()).AppendLine();

            if (!string.IsNullOrEmpty(message))
                text.AppendLine("Zdroj výjimky: " +message);
            text.AppendLine("Typ výjimky: " +ex.GetType().ToString()).AppendLine();

            text.AppendLine("Zpráva: " + ex.Message).AppendLine();

            text.AppendLine("Výpis stack trace" +ex.StackTrace);

                Action action = () =>
                {
                    bool ok = ReportError(text.ToString()); // send e-mail with the error message to me.
                    string tmp = ok ? "Luki už má hlášení o chybě na mailu. Ten bude mít radost." : "Chtěl sem Lukimu poslat hlášení, ale nepovedlo se to. Tak mu hlavně napiš, jinak ti už v životě neudělá tiramisu.";
                    MessageBox.Show(MainWindow, "Došlo k chybě a ta zkurvená hra spadla. " + tmp, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                };

                if (MainWindow != null)
                    MainWindow.Invoke(action);
                else
                    action();

                Environment.Exit(0);

            //if (TestMode)
            //    throw ex;
        }

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="sender">Source of the exception</param>
        /// <param name="e">The exception</param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
            => OnError(e.Exception);

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="sender">Source of the exception</param>
        /// <param name="e">The exception</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            => OnError((Exception)e.ExceptionObject, sender.ToString());

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
    }
}