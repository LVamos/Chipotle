using Game;

using Game.Audio;
using Game.Controls;
using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Debug;
using Game.Debug.UI;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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
			string version = Resources.Load<TextAsset>(MainScript.VersionPath).text;
			IntPtr window = FindWindowByName(null, Application.productName);
			if (window != IntPtr.Zero)
				SetWindowText(window, $"Chipotle {version}");
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
			try
			{
				return NativeInputDialog.ShowModal(prompt, title, defaultValue);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Shows a text in a native read-only edit box with an OK button and waits until it is closed.
		/// </summary>
		public static void ShowText(string text, string title)
		{
			try
			{
				NativeInputDialog.ShowReadOnly(text, title);
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		/// Shows a native input dialog without blocking the game. The game is paused until the dialog is closed.
		/// </summary>
		/// <param name="inputClosed">Receives the entered text, or null when the dialog was canceled.</param>
		public static void InputBox(string prompt, string title, string defaultValue, Action<string> inputClosed)
		{
			GameManager.PauseGame();
			NativeInputDialog.Show(prompt, title, defaultValue, text =>
			{
				GameManager.ResumeGame();
				inputClosed?.Invoke(text);
			});
		}

		public static void OpenDebugSettings()
		{
			var window = DebugSettingsWindow.CreateInstance();
			OpenModalWindow(window);
		}


		public static void ResetGame()
		{
			ScreenReader.Speak("Restartuju hru");
			Sounds.StopAllSounds(0);
			StartGame();
		}

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		private const int SW_RESTORE = 9;
		private const int SW_MINIMIZE = 6;
		private const int SW_SHOWNORMAL = 1;

		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentThreadId();

		[DllImport("user32.dll")]
		private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool attach);

		[DllImport("user32.dll")]
		private static extern bool BringWindowToTop(IntPtr hWnd);

		private static IntPtr FindOwnWindow(Func<string, bool> titleMatches, bool visibleOnly = true)
		{
			uint ownProcess = (uint)Process.GetCurrentProcess().Id;
			IntPtr found = IntPtr.Zero;
			EnumWindows((hWnd, _) =>
			{
				GetWindowThreadProcessId(hWnd, out uint processId);
				if (processId != ownProcess || (visibleOnly && !IsWindowVisible(hWnd)))
					return true;

				StringBuilder title = new(256);
				GetWindowText(hWnd, title, title.Capacity);
				if (!titleMatches(title.ToString()))
					return true;

				found = hWnd;
				return false;
			}, IntPtr.Zero);
			return found;
		}

		private static IntPtr _previousForeground;

		private static IntPtr FindGameWindow() => FindOwnWindow(t => t == "Running" || t == "Game", false);

		/// <summary>
		/// Checks that the game window is really the foreground window and Unity knows about it.
		/// </summary>
		public static bool IsGameWindowActive()
			=> Application.isFocused && (!Application.isEditor || GetForegroundWindow() == FindGameWindow());

		/// <summary>
		/// Activates the game window once more through the previously active window, so that Unity notices the focus change.
		/// </summary>
		public static void RefocusGameWindow()
		{
			if (!Application.isEditor)
				return;

			IntPtr game = FindGameWindow();
			if (game != IntPtr.Zero)
				ActivateGameWindow(game, _previousForeground);
		}

		private static void ActivateGameWindow(IntPtr game, IntPtr bounceVia)
		{
			// Activating from the main thread inside Update leaves Unity unfocused, so the keyboard stays disabled.
			new System.Threading.Thread(() =>
			{
				if (bounceVia != IntPtr.Zero)
				{
					SetForegroundWindow(bounceVia);
					System.Threading.Thread.Sleep(150);
				}

				uint foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), out _);
				uint ownThread = GetCurrentThreadId();
				bool attached = foregroundThread != ownThread && AttachThreadInput(ownThread, foregroundThread, true);
				ShowWindow(game, SW_SHOWNORMAL);
				BringWindowToTop(game);
				SetForegroundWindow(game);
				if (attached)
					AttachThreadInput(ownThread, foregroundThread, false);
			}) { IsBackground = true }.Start();
		}

		public static void FocusGameWindow()
		{
			if (Application.isEditor)
			{
				IntPtr main = FindOwnWindow(t => t.Contains(" - Unity "));
				if (main != IntPtr.Zero)
					ShowWindow(main, SW_MINIMIZE);

				IntPtr game = FindGameWindow();
				if (game == IntPtr.Zero)
					return;

				_previousForeground = GetForegroundWindow();
				ActivateGameWindow(game, IntPtr.Zero);
				return;
			}

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