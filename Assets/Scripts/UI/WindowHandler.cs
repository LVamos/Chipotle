using DavyKager;

using Game.Audio;
using Game.Controls;
using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Debug;
using Game.Debug.UI;

using Microsoft.VisualBasic;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Game.UI
{
	/// <summary>
	/// Manages user interface, redistributes keyboard input to virtual windows
	/// </summary>
	public static class WindowHandler
	{
		private static void ShowVersion()
		{
			IntPtr window = FindWindowByName(null, Application.productName); // najde okno podle původního titulku
			if (window != IntPtr.Zero)
			{
				SetWindowText(window, $"Chipotle {MainScript.Version}");
			}
		}
		[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
		static extern IntPtr FindWindowByName(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetWindowText(IntPtr hWnd, string lpString);


		private static void AddKeyboardHandler()
		{
			GameObject obj = new(nameof(KeyboardHandler));
			_keyboardHandler = obj.AddComponent<KeyboardHandler>();
		}

		private static KeyboardHandler _keyboardHandler;

		public static void Initialize()
		{
			CreateDebugManager();
			AddKeyboardHandler();
			AddDualSenseHandler();
			ShowVersion();
		}

		private static void AddDualSenseHandler()
		{
			GameObject obj = new(nameof(DualSenseHandler));
			_dualSenseHandler = obj.AddComponent<DualSenseHandler>();
		}

		private static DualSenseHandler _dualSenseHandler;

		private static void CreateDebugManager() => DebugManager = DebugManager.CreateInstance();

		public static string InputBox(string prompt, string title, string defaultValue = "")
		{
			string input = null;
			try
			{
				input = Interaction.InputBox(prompt, title, defaultValue);
			}
			catch (Exception) { }
			return input;
		}

		public static void OpenDebugSettings()
		{
			var window = DebugSettingsWindow.CreateInstance();
			OpenModalWindow(window);
		}


		public static void ResetGame()
		{
			Tolk.Speak("Restartuju hru");
			Sounds.StopAllSounds(0);
			StartGame();
		}

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private const int SW_RESTORE = 9;

		public static void FocusGameWindow()
		{
			IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;

			if (hwnd != IntPtr.Zero)
			{
				ShowWindow(hwnd, SW_RESTORE);
				SetForegroundWindow(hwnd);
			}
		}

		/// <summary>
		/// Handles the KeyPress message.
		/// </summary>
		/// <param name="letter">The key that was pressed</param>
		public static void OnKeyPress(char letter)
		{
			DebugManager.OnKeyPress(letter);
			ActiveWindow.OnKeyPress(letter);
		}

		/// <summary>
		/// Currently focused window
		/// </summary>
		public static VirtualWindow ActiveWindow { get; private set; }

		/// <summary>
		/// Reference to previously active window
		/// </summary>
		public static VirtualWindow PreviousWindow { get; private set; }

		/// <summary>
		/// Runs the main menu.
		/// </summary>
		public static void MainMenu() => Switch(MainMenuWindow.CreateInstance());

		/// <summary>
		/// Runs a voice menu
		/// </summary>
		/// <param name="items">Items for the menu</param>
		/// <param name="introText">Text to announce when menu is activated</param>
		/// <param name="wrappingAllowed">Enables or disables menu wrapping</param>
		/// <param name="introSound">Name of a sound to be played when the menu is activated</param>
		/// <param name="outroSound">Name of a sound to be played when menu is closed</param>
		/// <param name="selectionSound">Name of a sound to be played when user selects an item</param>
		/// <param name="wrapDownSound">Name of a sound to be played when the menu wraps to lower edge</param>
		/// <param name="wrapUpSound">Name of a sound to be played when the menu wraps to upper edge</param>
		/// <param name="upperEdgeSound">Name of a sound to be played when cursor gets to upper edge of the menu</param>
		/// <param name="lowerEdgeSound">Name of a sound to be played when cursor gets to lower edge of the menu</param>
		/// <returns>Tuple with index of selected item and value of selected item</returns>
		public static int Menu(MenuParameters parameters, bool openAsModal = true)
		{
			MenuWindow menu = MenuWindow.CreateInstance(parameters);

			if (openAsModal)
				OpenModalWindow(menu);
			else Switch(menu, false);
			return 0;
		}

		/// <summary>
		/// Delegates event to event handler of active window
		/// </summary>
		/// <param name="inpuut">Event parameters</param>
		public static void OnKeyDown(KeyboardInput inpuut)
		{
			if (InputConfig.KeyboardRebinding)
			{
				InputConfig.CatchKeysForBinding(inpuut);
				_catchKeyboardKeysForBinding = true;
				return;
			}

			DebugManager.OnKeyDown(inpuut);
			ActiveWindow?.OnKeyDown(inpuut);
		}

		/// <summary>
		/// Sends the KeyUp event to the current active window.
		/// </summary>
		public static void OnKeyUp(KeyboardInput input)
		{
			if (InputConfig.KeyboardRebinding)
			{
				if (_catchKeyboardKeysForBinding && !_dualSenseHandler.AnyKeyPressed())
				{
					InputConfig.FinishKeyboardBinding();
					_catchKeyboardKeysForBinding = false;
					return;
				}
			}

			DebugManager.OnKeyUp(input);
			ActiveWindow?.OnKeyUp(input);
		}

		public static DebugManager DebugManager;
		private static bool _catchKeyboardKeysForBinding;
		private static bool _catchDualSenseButtonsForBinding;

		/// <summary>
		/// Opens virtual modal window
		/// </summary>
		/// <param name="modalWindow">Reference to new window</param>
		public static void OpenModalWindow(VirtualWindow modalWindow)
		{
			PreviousWindow = ActiveWindow;
			ActiveWindow = modalWindow;
			ActiveWindow.ParentWindow = PreviousWindow;
			ActiveWindow.OnActivate();
		}

		/// <summary>
		/// Begins the game.
		/// </summary>
		public static void StartGame()
		{
			Switch(GameWindow.CreateInstance());
			World.StartGame();
		}

		/// <summary>
		/// Closes currently active window and activates another one.
		/// </summary>
		/// <param name="window">New window</param>
		public static void Switch(VirtualWindow window, bool keepPreviousWindow = true)
		{
			ActiveWindow?.OnDeactivate(); // Let active window react on deactivating

			PreviousWindow = keepPreviousWindow ? ActiveWindow : null; // Backing up for future use
			ActiveWindow = window;
			ActiveWindow.OnActivate(); // Let new window react on activation
		}

		public static void OnKeyDown(DualSenseInput input)
		{
			if (InputConfig.DualSenseRebinding)
			{
				InputConfig.CatchKeysForBinding(input);
				_catchDualSenseButtonsForBinding = true;
				return;
			}

			ActiveWindow?.OnKeyDown(input);
		}


		public static void OnKeyUp(DualSenseInput input)
		{
			if (InputConfig.DualSenseRebinding)
			{
				if (_catchDualSenseButtonsForBinding && !_dualSenseHandler.AnyKeyPressed())
				{
					InputConfig.FinishDualSenseBinding();
					_catchDualSenseButtonsForBinding = false;
					return;
				}
			}

			ActiveWindow?.OnKeyUp(input);
		}

		public static KeyboardInput? CatchKeyboardShortcut()
		{
			throw new NotImplementedException();
		}

		internal static DualSenseInput? CatchDualSenseShortcut()
		{
			throw new NotImplementedException();
		}
	}
}