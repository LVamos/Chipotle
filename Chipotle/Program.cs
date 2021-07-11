using System;
using System.Windows.Forms;
using DavyKager;
using Luky;
using Game.UI;
using Game.Terrain;
using System.IO;
using System.Media;

namespace Game
{
 class Program : DebugSO
 {
  const bool _abort = false;

  static public MainWindow MainWindow { get; private set; }

  [STAThread]
  static void Main()
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

  static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
  => OnError(e.ExceptionObject.ToString()); 

  static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
   => OnError(e.Exception);

  public static void OnError(Exception ex)
  => OnError(ex.ToString()); 

  static void OnError(string error)
  {
    Action action = () => MessageBox.Show(MainWindow, error, AppDomain.CurrentDomain.FriendlyName + ": závažná chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (MainWindow != null)
                MainWindow.Invoke(action);
            else action();

   Environment.Exit(0);
  }
 } 
}
