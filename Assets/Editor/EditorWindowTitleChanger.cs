using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using UnityEditor;


[InitializeOnLoad]
public class EditorWindowTitleChanger
{
	private static string originalTitle;
	private static bool isTitleSet = false;

	static EditorWindowTitleChanger()
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		EditorApplication.quitting += ResetTitle;
		AppDomain.CurrentDomain.UnhandledException += (sender, args) => ResetTitle();
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.EnteredPlayMode)
		{
			SetRunningTitle();
			EditorApplication.delayCall += MinimizeMainWindow;
		}
		else if (state == PlayModeStateChange.ExitingPlayMode)
		{
			ResetTitle();
			RestoreMainWindow();
		}
	}

	private const int SW_MINIMIZE = 6;
	private const int SW_SHOWNORMAL = 1;
	private const int SW_RESTORE = 9;

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
	private static extern bool ShowWindow(IntPtr hWnd, int command);

	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	private static IntPtr FindWindow(Func<string, bool> titleMatches, bool visibleOnly = true)
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

	private static bool IsMainWindowTitle(string title) => title.Contains(" - Unity ");

	private const double FocusRetryDuration = 20;
	private const double FocusRetryInterval = 0.5;
	private static double _focusUntil;
	private static double _nextFocusAttempt;

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	private const byte VK_MENU = 0x12;
	private const uint KEYEVENTF_KEYUP = 2;

	[DllImport("user32.dll")]
	private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extraInfo);

	private static void MinimizeMainWindow()
	{
		_focusUntil = EditorApplication.timeSinceStartup + FocusRetryDuration;
		_nextFocusAttempt = 0;
		EditorApplication.update -= KeepGameWindowFocused;
		EditorApplication.update += KeepGameWindowFocused;
	}

	private static void KeepGameWindowFocused()
	{
		double now = EditorApplication.timeSinceStartup;
		if (now > _focusUntil || !EditorApplication.isPlaying)
		{
			EditorApplication.update -= KeepGameWindowFocused;
			return;
		}

		if (now < _nextFocusAttempt)
			return;
		_nextFocusAttempt = now + FocusRetryInterval;

		IntPtr main = FindWindow(IsMainWindowTitle);
		if (main != IntPtr.Zero)
			ShowWindow(main, SW_MINIMIZE);

		IntPtr game = FindWindow(t => t == "Running" || t == "Game", false);
		if (game == IntPtr.Zero || (IsWindowVisible(game) && GetForegroundWindow() == game))
			return;

		keybd_event(VK_MENU, 0, 0, UIntPtr.Zero);
		keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
		ShowWindow(game, SW_SHOWNORMAL);
		SetForegroundWindow(game);
	}

	private static void RestoreMainWindow()
	{
		IntPtr main = FindWindow(IsMainWindowTitle);
		if (main != IntPtr.Zero)
			ShowWindow(main, SW_RESTORE);
	}

	private static void SetRunningTitle()
	{
		if (!isTitleSet)
		{
			originalTitle = UnityEditor.EditorWindow.focusedWindow?.titleContent.text;
			if (originalTitle != null)
			{
				UnityEditor.EditorWindow.focusedWindow.titleContent.text = "Running";
				isTitleSet = true;
			}
		}
	}

	private static void ResetTitle()
	{
		if (isTitleSet && originalTitle != null)
		{
			UnityEditor.EditorWindow.focusedWindow.titleContent.text = originalTitle;
			isTitleSet = false;
		}
	}
}
