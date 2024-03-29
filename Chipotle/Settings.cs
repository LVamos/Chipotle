using OpenTK;

namespace Game
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
			PlayCutscenes = false;
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
}
