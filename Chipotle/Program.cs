using DavyKager;

using Game.UI;

using Luky;

using Microsoft.VisualBasic;

using OpenTK;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

namespace Game
{
    /// <summary>
    /// Entry point of the application
    /// </summary>
    public static class Program
    {
        public static class Settings
        {
            /// <summary>
            /// Specifies if the JAWS key hook should be disabled.
            /// </summary>
            public static bool DisableJawsKeyHook;

            /// <summary>
            /// Enables some test methods.
            /// </summary>
            public static bool TestCommandsEnabled;

            /// <summary>
            /// Enables or disables cutscenes.
            /// </summary>
            public static bool PlayCutscenes;

            /// <summary>
            /// Custom initial position for Tuttle
            /// </summary>
            public static Vector2? TuttleTestStart;

            /// <summary>
            /// Enables or disables a custom initial posiiton for Tuttle.
            /// </summary>
            public static bool AllowTuttlesCustomPosition;

            /// <summary>
            /// Enables or disables a custom initial position for Chipotle.
            /// </summary>
            public static bool AllowCustomChipotlesStartPosition;

            /// <summary>
            /// Enables or disables commands for loading and creating predefined saves.
            /// </summary>
            public static bool AllowPredefinedSaves;

            /// <summary>
            /// Enables or disables error reporting.
            /// </summary>
            public static bool ReportErrors;

            /// <summary>
            /// Enables or disables raising unhandled exceptions.
            /// </summary>
            public static bool ThrowExceptions;

            /// <summary>
            /// Enables or disables music in main menu.
            /// </summary>
            public static bool PlayMenuLoop;

            /// <summary>
            /// Enables or disables main menu at startup.
            /// </summary>
            public static bool MainMenuAtStartup;

            /// <summary>
            /// Enables or disables sending Tuttle to the pool locality at startup.
            /// </summary>
            public static bool SendTuttleToPool;

            /// <summary>
            /// Enables or disables Tuttle's Chipotle following.
            /// </summary>
            public static bool LetTuttleFollowChipotle;

            /// <summary>
            /// Switches the game to debug mode.
            /// </summary>
            public static void SetDebugMode()
            {
                TuttleTestStart = new Vector2(1023, 1030);
                AllowTuttlesCustomPosition = false;
                AllowCustomChipotlesStartPosition = true;
                AllowPredefinedSaves = true;
                DisableJawsKeyHook = false;
                LetTuttleFollowChipotle = true;
                MainMenuAtStartup = false;
                PlayCutscenes = true;
                PlayMenuLoop = false;
                ReportErrors = false;
                SendTuttleToPool = false;
                TestCommandsEnabled = true;
                ThrowExceptions = true;
            }

            /// <summary>
            /// Prepares the game for release.
            /// </summary>
            public static void SetReleaseMode()
            {
                AllowTuttlesCustomPosition = false;
                AllowCustomChipotlesStartPosition = false;
                AllowPredefinedSaves = false;
                DisableJawsKeyHook = true;
                LetTuttleFollowChipotle = true;
                MainMenuAtStartup = true;
                PlayCutscenes = true;
                PlayMenuLoop = true;
                ReportErrors = true;
                SendTuttleToPool = true;
                TestCommandsEnabled = false;
                ThrowExceptions = false;
            }

            /// <summary>
            /// Prepares the game for testing purposes.
            /// </summary>
            public static void SetTestMode()
            {
                AllowTuttlesCustomPosition = false;
                AllowCustomChipotlesStartPosition = false;
                AllowPredefinedSaves = true;
                DisableJawsKeyHook = true;
                LetTuttleFollowChipotle = true;
                MainMenuAtStartup = true;
                PlayCutscenes = true;
                PlayMenuLoop = true;
                ReportErrors = true;
                SendTuttleToPool = true;
                TestCommandsEnabled = false;
                ThrowExceptions = false;
            }


        }

        /// <summary>
        /// Path to a map file
        /// </summary>
        public static string MapPath => Path.Combine(Program.DataPath, @"Map\chipotle.xml");


        private static string GetUserInfo()
        {
            StringBuilder text = new StringBuilder();

            if (World.Player != null)
            {
                Vector2 position = World.Player.Area.Center;
                text.AppendLine("Verze: " + Version);
                text.AppendLine("Pozice: " + position.ToString());
                text.AppendLine("Lokace: " + World.Player.Locality.Name.Indexed);

                // Get nearest objects
                string[] objectList = World.GetNearestObjects(position).Where(o => o.Area.GetDistanceFrom(position) < 10).Select(o => o.Name.Indexed).ToArray<string>();
                string objects = objectList.IsNullOrEmpty() ? "žádné" : string.Join(", ", objectList);
                text.AppendLine("Okolní objekty: " + objects);
            }
            text.AppendLine("Datum a čas: " + DateTime.Now.ToString());

            return text.ToString();
        }

        public static void SendFeedback()
        {
            Program.MainWindow.GameInProgress = false;
            string message = Microsoft.VisualBasic.Interaction.InputBox("", "Zadej zprávu pro autora", "");
            Program.MainWindow.GameInProgress = true;

            if (string.IsNullOrEmpty(message))
            {
                Tolk.Speak("Tak nic no");
                return;
            }

            string subject = "Chipotle: " + System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName + " posílá připomínku";
            string body = "Zpráva: " + message + Environment.NewLine + GetUserInfo();

            if (!SendMeMail(subject, body))
                Tolk.Speak("Zprávu se nepodařilo odeslat.");
            else Tolk.Speak("Odesláno");
        }

        /// <summary>
        /// Current version of the game
        /// </summary>
        public static string Version = "0.17";

        /// <summary>
        /// Sends an e-mail message to my Gmail account.
        /// </summary>
        /// <param name="subject">Subject for the message</param>
        /// <param name="body">Body of the message</param>
        /// <returns>True if the message is sent properly</returns>
        private static bool SendMeMail(string subject, string body, Attachment attachment = null)
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
            catch (Exception e)
            {
                throw e;
                return false;
            }
            return true;
        }

        /// <summary>
        /// sends a critical error announcement to authzor's mail address.
        /// </summary>
        /// <param name="message">A description of the problem from an user</param>
        /// <returns>True if it¨s successful</returns>
        private static bool ReportError(string message, bool addUserInfo = true)
        {
            string path = @"data\game.sav";
            Attachment attachment = File.Exists(path) ? new Attachment(path) : null;
            string subject = "Chipotle: " + System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName + " hlásí kritickou chybu";
            string body = message + Environment.NewLine + GetUserInfo();
            return SendMeMail(subject, body, attachment);
        }

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
        public static string SerializationPath => Path.Combine(DataPath, "game.sav");

        /// <summary>
        /// Path to the folder with predefined saves.
        /// </summary>
        public static string PredefinedSavesPath
        { get => Path.Combine(DataPath, "Saves"); }

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="ex">The exception</param>
        public static void OnError(Exception ex, string message = null)
        {
            EnableJAWSKeyHook(); // Restore JAWS key hook


            Action action = () =>
            {
                // Ask for problem description
                string description = Microsoft.VisualBasic.Interaction.InputBox("", "          Ta zkurvená hra spadla! Napiš Lukimu, co se stalo, než mu definitivně jebne.", "");

                // Prepare an error message
                StringBuilder text = new StringBuilder(GetUserInfo());

                if (!string.IsNullOrEmpty(description))
                    text.AppendLine("Zpráva: " + description);

                if (!string.IsNullOrEmpty(message))
                    text.AppendLine("Zdroj výjimky: " + message);

                text.AppendLine("Typ výjimky: " + ex.GetType().ToString()).AppendLine();
                text.AppendLine(ex.Message).AppendLine();
                text.AppendLine("Stack trace" + Environment.NewLine + ex.StackTrace);

                string report = text.ToString();
                bool ok = ReportError(report); // send e-mail with the error message to me.
                if (!ok)
                    File.WriteAllText(_errorReportPath, report);

                report = ok ? "Už to má na mailu. Ten bude mít radost." : "Nepovedlo se to. Hlášení se odešle při příštím spuštění.";
                MessageBox.Show(MainWindow, report, "", MessageBoxButtons.OK);
            };

            if (Settings.ReportErrors)
            {
                if (MainWindow != null)
                    MainWindow.Invoke(action);
                else
                    action();
            }

            if (Settings.ThrowExceptions)
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
            ResendErrorReport();
            Settings.SetDebugMode();


            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

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
        private static void ResendErrorReport()
        {
            if (!File.Exists(_errorReportPath))
                return;

            string body = null;
            try
            {
                body = File.ReadAllText(_errorReportPath);
            }
            catch (Exception e)
            {
                return;
            }

            string subject = "Chipotle: " + System.DirectoryServices.AccountManagement.UserPrincipal.Current.DisplayName + " hlásí kritickou chybu";
            if (SendMeMail(subject, body))
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
            if (!Settings.DisableJawsKeyHook || !InitJAWS())
                return;

            _jaws.InvokeMember("Enable", System.Reflection.BindingFlags.InvokeMethod, null, _jawsObject, new object[] { true });
        }

        /// <summary>
        /// Removes JAWS keyhook.
        /// </summary>
        public static void DisableJAWSKeyHook()
        {
            if (!Settings.DisableJawsKeyHook || !InitJAWS())
                return;

            _jaws.InvokeMember("Disable", System.Reflection.BindingFlags.InvokeMethod, null, _jawsObject, null);
        }

        /// <summary>
        /// Displays an error announcement and terminates the program.
        /// </summary>
        /// <param name="prompt">The prompt to be shown</param>
        public static void Terminate(string prompt)
        {
            World.Sound.StopAll();
            string message = string.IsNullOrEmpty(prompt) ? "Došlo k neznámé chybě. Hra bude ukončena." : prompt;
            Interaction.MsgBox(prompt, MsgBoxStyle.Critical, "Chyba");
            EnableJAWSKeyHook();
            Environment.Exit(0);
        }
    }
}