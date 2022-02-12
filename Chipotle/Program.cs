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
        public static string Version = "0.10";

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
        public const bool TestMode = true;

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


        public static Vector2? TuttleTestStart = new Vector2(1070, 1058);

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="ex">The exception</param>
        public static void OnError(Exception ex, string message = null)
        {
                EnableJAWSKeyHook(); // Restore JAWS key hook


                Action action = () =>
                {
                    // Ask for comment
                    string comment = string.Empty;
                DialogResult result = MessageBox.Show(MainWindow, "Ta zkurvená hra spadla. Posílám Lukymu hlášení. Chceš k tomu přidat komentář?", "Chyba", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if(result == DialogResult.Yes)
                    comment = Microsoft.VisualBasic.Interaction.InputBox("", "Zadej popis chyby", "");

                    // Prepare an error message
                    StringBuilder text = new StringBuilder();
                    text.AppendLine("Uživatel: " + Environment.UserName)
                    .AppendLine(comment)
                    .AppendLine("Verze: " + Version)
                    .AppendLine("Datum a čas: " + DateTime.Now.ToString());

                    if (World.Player != null)
                        text.AppendLine("Aktuální pozice Chipotla: " + World.Player.Area.Center.ToString()).AppendLine();

                    if (!string.IsNullOrEmpty(message))
                        text.AppendLine("Zdroj výjimky: " + message);

                    text.AppendLine("Typ výjimky: " + ex.GetType().ToString()).AppendLine();
                    text.AppendLine("Zpráva: " + ex.Message).AppendLine();
                    text.AppendLine("Výpis stack trace" + ex.StackTrace);

                    string report = text.ToString();
                    bool ok = ReportError(report); // send e-mail with the error message to me.
                    if (!ok)
                        File.WriteAllText(_errorReportPath, report);

                    report = ok ? "Už to má na mailu. Ten bude mít radost." : "Nepovedlo se to. Hlášení se odešle při příštím spuštění.";
                    MessageBox.Show(MainWindow, report, "", MessageBoxButtons.OK);
                };

            if (!TestMode)
            {
                if (MainWindow != null)
                    MainWindow.Invoke(action);
                else
                    action();
            }

            //if (TestMode)
            throw ex;

            Environment.Exit(0);

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
            SendDelayedReport();
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
        /// Path to a file containing a delayed error report.
        /// </summary>
        private const string _errorReportPath = @"data\error.dat";

        /// <summary>
        /// Sends a delayed error report from previous execution if it's saved on the disc.
        /// </summary>
        private static void SendDelayedReport()
        {
            if (!File.Exists(_errorReportPath))
                return;

                string message = null;
                try
                {
                    message = File.ReadAllText(_errorReportPath);
                }
                catch (Exception e)
                {
                    return;
                }

                if (ReportError(message))
                    File.Delete(_errorReportPath);
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