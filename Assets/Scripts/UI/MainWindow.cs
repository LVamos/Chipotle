using DavyKager;

using System;

namespace Game.UI
{
	/// <summary>
	/// The only visible window which catches keyboard events.
	/// </summary>
	public class MainWindow
	{

		protected void OnLoad()
		{
			if (!Settings.MainMenuAtStartup)
				WindowHandler.StartGame();
			else
				WindowHandler.MainMenu();
		}

		protected void OnUpdateFrame()
		{
			// Herní smyčka
			World.UpdateGame();
		}

		protected void OnUnload()
		{
			MainScript.EnableJAWSKeyHook();
		}

		/// <summary>
		/// Handler of the KeyDown event
		/// </summary>
		private void OnKeyDown(KeyShortcut shortcut)
		{
			InterruptSpeech();
			WindowHandler.OnKeyDown(shortcut);
		}

		/// <summary>
		/// Handler of the KeyUp event
		/// </summary>
		private void OnKeyUp(KeyShortcut e)
		{
			WindowHandler.OnKeyUp(e);
		}

		/// <summary>
		/// Interrupts an ongoing SAPI or screen reader utterance.
		/// </summary>
		/// <remarks>Works with SAPI and JAWS only. NVDA does this automatically.</remarks>
		private void InterruptSpeech()
		{
			Tolk.Silence();

			if (Tolk.DetectScreenReader() == "JAWS")
				Tolk.Speak(string.Empty, true);
		}

		private void OnActivated()
		{
			MainScript.DisableJAWSKeyHook();

			//todo if (Sounds.Muted)
			{
				World.ResumeCutscene();
				//todo Sound.Unmute();
			}
		}

		private void OnDeactivate()
		{
			MainScript.EnableJAWSKeyHook();
			World.PauseCutscene();
			//todo Sound.Mute();
		}

		protected void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (World.GameInProgress)
				World.SaveGame();

			Environment.Exit(0);
		}

	}
}