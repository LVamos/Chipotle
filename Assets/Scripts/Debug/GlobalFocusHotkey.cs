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
			if (_hookId != IntPtr.Zero) return;

			_callback = HookCallback;
			_hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _callback, GetModuleHandle(null), 0);
		}

		/// <summary>
		/// Uninstalls the global keyboard hook.
		/// </summary>
		public static void Uninstall()
		{
			if (_hookId == IntPtr.Zero) return;

			UnhookWindowsHookEx(_hookId);
			_hookId = IntPtr.Zero;
			_callback = null;
		}

		private static IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
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
						WindowHandler.FocusGameWindow();
				}
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