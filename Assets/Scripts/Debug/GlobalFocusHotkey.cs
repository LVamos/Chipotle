using Game.UI;

using System;
using System.Runtime.InteropServices;

namespace Game.Debug
{
	/// <summary>
	/// Installs a global low-level keyboard hook and focuses the game window on Ctrl+Alt+Shift+C.
	/// </summary>
	public static class GlobalFocusHotkey
	{
		private static IntPtr _hookId = IntPtr.Zero;
		private static LowLevelKeyboardProc _callback;

		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_SYSKEYDOWN = 0x0104;
		private const int VK_SHIFT = 0x10;
		private const int VK_CONTROL = 0x11;
		private const int VK_MENU = 0x12;
		private const int VK_C = 0x43;

		private delegate IntPtr LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam);

		[StructLayout(LayoutKind.Sequential)]
		private struct KBDLLHOOKSTRUCT
		{
			public uint vkCode;
			public uint scanCode;
			public uint flags;
			public uint time;
			public IntPtr dwExtraInfo;
		}

		/// <summary>
		/// Installs the global keyboard hook. Call once during startup.
		/// </summary>
		public static void Install()
		{
			try
			{
				if (_hookId != IntPtr.Zero) return;

				_callback = HookCallback;
				_hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _callback, GetModuleHandle(null), 0);

				if (_hookId == IntPtr.Zero)
				{
					int errorCode = Marshal.GetLastWin32Error();
					UnityEngine.Debug.LogWarning($"Failed to install keyboard hook. Error code: {errorCode}");
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError($"Exception while installing keyboard hook: {ex}");
			}
		}

		/// <summary>
		/// Uninstalls the global keyboard hook.
		/// </summary>
		public static void Uninstall()
		{
			try
			{
				if (_hookId == IntPtr.Zero) return;

				bool success = UnhookWindowsHookEx(_hookId);
				if (!success)
				{
					int errorCode = Marshal.GetLastWin32Error();
					UnityEngine.Debug.LogWarning($"Failed to uninstall keyboard hook. Error code: {errorCode}");
				}

				_hookId = IntPtr.Zero;
				_callback = null;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError($"Exception while uninstalling keyboard hook: {ex}");
			}
		}

		private static IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
		{
			try
			{
				if (code >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
				{
					KBDLLHOOKSTRUCT data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
					if (data.vkCode == VK_C)
					{
						bool shift = (GetKeyState(VK_SHIFT) & 0x8000) != 0;
						bool control = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
						bool alt = (GetKeyState(VK_MENU) & 0x8000) != 0;

						if (shift && control && alt)
						{
							try
							{
								WindowHandler.FocusGameWindow();
							}
							catch (Exception ex)
							{
								// Log but don't throw - we're in a hook callback
								UnityEngine.Debug.LogError($"Error in FocusGameWindow: {ex}");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Never throw from a hook callback - it will crash the app
				UnityEngine.Debug.LogError($"Error in keyboard hook: {ex}");
			}

			return CallNextHookEx(_hookId, code, wParam, lParam);
		}

		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hMod, uint threadId);

		[DllImport("user32.dll")]
		private static extern bool UnhookWindowsHookEx(IntPtr hook);

		[DllImport("user32.dll")]
		private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern short GetKeyState(int virtualKey);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string moduleName);
	}
}