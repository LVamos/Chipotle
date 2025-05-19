using DavyKager;

using Game.Audio;

using Microsoft.VisualBasic;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

using UnityEngine;
using UnityEngine.SceneManagement;

using Object = System.Object;

namespace Game
{

    /// <summary>
    /// Třída MainScript reaguje na události, které vyvolává Unity runtime. 
    /// </summary>
    public static class MainScript
    {
        public static bool GameLoaded;
        public static string SoundPath => Path.Combine(DataPath, "Sounds");

        /// <summary>
        /// Path to a YAML file with item definitions.
        /// </summary>
        public static string ItemsPath => Path.Combine(DataPath, @"Items\items.yaml");

        /// <summary>
        /// Path to a YAML file with passage definitions.
        /// </summary>
        public static string PassagesPath => Path.Combine(DataPath, @"Passages\passages.yaml");

        public static string MaterialsPath => Path.Combine(DataPath, @"Materials\materials.yaml");

        /// <summary>
        /// Path to a map file
        /// </summary>
        public static string MapPath => Path.Combine(DataPath, "Map");

        /// <summary>
        /// Path to config folder.
        /// </summary>
        public static string ConfigPath => Path.Combine(DataPath, "Config");

        private static string GetUserInfo()
        {
            StringBuilder text = new();

            if (World.Player != null)
            {
                Vector2 position = World.Player.Area.Value.Center;
                text.AppendLine("Verze: " + Version);
                text.AppendLine("Pozice: " + position.ToString());
                text.AppendLine("Lokace: " + World.Player.Zone.Name.Indexed);

                // Get nearest objects
                string[] objectList = World.GetNearestObjects(position)
                    .Where(o => o.Area.Value.GetDistanceFrom(position) < 10).Select(o => o.Name.Indexed).ToArray<string>();
                string objects = objectList.IsNullOrEmpty() ? "žádné" : string.Join(", ", objectList);
                text.AppendLine("Okolní objekty: " + objects);
            }

            text.AppendLine("Datum a čas: " + DateTime.Now.ToString());

            return text.ToString();
        }

        public static void SendFeedback()
        {
            World.GameInProgress = false;
            string message = Interaction.InputBox("", "Zadej zprávu pro autora", "");
            World.GameInProgress = true;

            if (string.IsNullOrEmpty(message))
            {
                Tolk.Speak("Tak nic no");
                return;
            }

            string subject = null;
            //string subject = "Chipotle: " + WindowsIdentity.GetCurrent().Name + " posílá připomínku";
            string body = "Zpráva: " + message + Environment.NewLine + GetUserInfo();
            Attachment attachment = GetAttachment();

            if (!SendMeMail(subject, body, attachment))
                Tolk.Speak("Zprávu se nepodařilo odeslat.");
            else
                Tolk.Speak("Odesláno");
        }

        private static Attachment GetAttachment()
        {
            try
            {
                byte[] logBytes = File.ReadAllBytes(LogPath);
                MemoryStream stream = new(logBytes);
                Attachment attachment = new(stream, "log.html", "text/html");
                return attachment;
            }
            catch (Exception)
            {
                return null;
            }
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
            MailAddress fromAddress = new("lukas.vamos@gmail.com", Environment.UserName);
            MailAddress toAddress = new("lukas.vamos@gmail.com", "Lukáš Vámoš");
            const string fromPassword = "yhfk gdui kund nfjm";

            SmtpClient smtp = new()
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
                using MailMessage message = new(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                };
                if (attachment != null)
                    message.Attachments.Add(attachment);

                smtp.Send(message);
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
        public static bool ReportError(string message, bool addUserInfo = true)
        {
            Attachment attachment = GetAttachment();
            string subject = null;
            //string subject=  "Chipotle: " + WindowsIdentity.GetCurrent().Name + " hlásí kritickou chybu";
            string body = message + Environment.NewLine + GetUserInfo();
            return SendMeMail(subject, body, attachment);
        }

        /// <summary>
        /// Path to data folder
        /// </summary>
        public static readonly string DataPath = @"Assets\Resources\Data\";
        public static readonly string LogPath = @"Assets\Resources\Data\Logs\log.html";

        /// <summary>
        /// Path to file used for serialization.
        /// </summary>
        public static string SerializationPath => Path.Combine(DataPath, "game.sav");

        /// <summary>
        /// Path to the folder with predefined saves.
        /// </summary>
        public static string PredefinedSavesPath => Path.Combine(DataPath, "Saves");

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="ex">The exception</param>
        public static void OnError(Exception ex, string message = null)
        {
            GUIUtility.systemCopyBuffer = ex.ToString();
            if (Settings.ThrowExceptions)
                throw ex;
            LogException(ex);
            EnableJAWSKeyHook(); // Restore JAWS key hook

            if (!Settings.ReportErrors || !GameLoaded)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
Application.Quit();
#endif
            }

            Action action = () =>
            {
                // Ask for problem description
                string description = Microsoft.VisualBasic.Interaction.InputBox("", "          Ta zkurvená hra spadla! Napiš Lukimu, co se stalo, než mu definitivně jebne.", "");

                // Prepare an error message
                StringBuilder text = new(GetUserInfo());

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

                report = ok
                    ? "Už to má na mailu. Ten bude mít radost."
                    : "Nepovedlo se to. Hlášení se odešle při příštím spuštění.";
                Interaction.MsgBox(report, MsgBoxStyle.OkOnly, "");
            };

            if (Settings.ReportErrors)
                action();

            if (Settings.ThrowExceptions)
                throw ex;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
Application.Quit();
#endif
        }

        private static void LogException(Exception ex) => Logger.LogError("Výjimka", ex.ToString());

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="sender">Source of the exception</param>
        /// <param name="e">The exception</param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) => OnError(e.Exception);

        /// <summary>
        /// An error handler
        /// </summary>
        /// <param name="sender">Source of the exception</param>
        /// <param name="e">The exception</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => OnError((Exception)e.ExceptionObject, sender?.ToString());

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
            catch (Exception)
            {
                return;
            }

            string subject = null;
            //string subject = "Chipotle: " + WindowsIdentity.GetCurrent().Name + " hlásí kritickou chybu";
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

            _jaws.InvokeMember("Enable", System.Reflection.BindingFlags.InvokeMethod, null, _jawsObject,
                new object[] { true });
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
            string message = string.IsNullOrEmpty(prompt) ? "Došlo k neznámé chybě. Hra bude ukončena." : prompt;
            //Interaction.MsgBox(prompt, MsgBoxStyle.Critical, "Chyba");
            EnableJAWSKeyHook();
            Environment.Exit(0);
        }

        /// <summary>
        /// Metoda AfterSceneLoad se spustí pokaždé, když Unity načte nějakou scénu.
        /// Unity vždy má nějakou scénu, takže ji nemusíme vytvářet, a využijeme již nějakou poskytnutou.
        /// </summary>
        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void AfterSceneLoad()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => OnError((Exception)e.ExceptionObject);

            try
            {
                Tolk.Load();
                Tolk.TrySAPI(false);
                DisableJAWSKeyHook();
                ResendErrorReport();
                Settings.LoadSettings();
                Sounds.LoadClips();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            EnableJAWSKeyHook();

            GameObject obj = new();
            obj.AddComponent<AutoPlayBehaviour>();
        }

        public static Scene Scene { get; set; }

        public static GameObject ItemFactory { get; set; }
    }
}