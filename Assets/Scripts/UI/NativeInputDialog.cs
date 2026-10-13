using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Game.UI
{
	/// <summary>
	/// A standard Windows dialog with a text box, OK and Cancel buttons built from native controls
	/// so that screen readers handle it the usual way. It runs on its own thread and never blocks Unity.
	/// </summary>
	public static class NativeInputDialog
	{
		/// <summary>
		/// Shows the dialog and reports the result on the calling (main) thread.
		/// </summary>
		/// <param name="onClosed">Receives the entered text, or null when the dialog was canceled.</param>
		public static void Show(string prompt, string title, string defaultValue, Action<string> onClosed)
		{
			SynchronizationContext context = SynchronizationContext.Current;
			byte[] template = BuildTemplate(prompt, title);
			IntPtr owner = GetGameWindow();

			Thread thread = new(() =>
			{
				string result = RunDialog(template, defaultValue ?? string.Empty, owner);
				if (context != null)
					context.Post(_ => onClosed?.Invoke(result), null);
				else
					onClosed?.Invoke(result);
			})
			{
				IsBackground = true,
				Name = "NativeInputDialog"
			};
			thread.Start();
		}

		/// <summary>
		/// Shows the dialog and waits for it on the calling thread. The game is frozen until the dialog is closed.
		/// </summary>
		/// <returns>The entered text, or null when the dialog was canceled.</returns>
		public static string ShowModal(string prompt, string title, string defaultValue)
			=> RunDialog(BuildTemplate(prompt, title), defaultValue ?? string.Empty, GetGameWindow());

		private const int IdOk = 1;
		private const int IdCancel = 2;
		private const int IdEdit = 1001;

		private const uint WM_CLOSE = 0x0010;
		private const uint WM_INITDIALOG = 0x0110;
		private const uint WM_COMMAND = 0x0111;
		private const uint EM_SETSEL = 0x00B1;

		private const uint WS_POPUP = 0x80000000;
		private const uint WS_CHILD = 0x40000000;
		private const uint WS_VISIBLE = 0x10000000;
		private const uint WS_CAPTION = 0x00C00000;
		private const uint WS_SYSMENU = 0x00080000;
		private const uint WS_BORDER = 0x00800000;
		private const uint WS_TABSTOP = 0x00010000;
		private const uint DS_MODALFRAME = 0x80;
		private const uint DS_SETFONT = 0x40;
		private const uint SS_NOPREFIX = 0x80;
		private const uint ES_AUTOHSCROLL = 0x80;
		private const uint BS_DEFPUSHBUTTON = 0x01;

		private const ushort ClassButton = 0x0080;
		private const ushort ClassEdit = 0x0081;
		private const ushort ClassStatic = 0x0082;

		private delegate IntPtr DialogProc(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr DialogBoxIndirectParam(
			IntPtr instance, IntPtr template, IntPtr parent, DialogProc dialogProc, IntPtr initParam);

		[DllImport("user32.dll")]
		private static extern bool EndDialog(IntPtr hwnd, IntPtr result);

		[DllImport("user32.dll")]
		private static extern IntPtr GetDlgItem(IntPtr hwnd, int id);

		[DllImport("user32.dll")]
		private static extern IntPtr SetFocus(IntPtr hwnd);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool SetDlgItemText(IntPtr hwnd, int id, string text);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetWindowTextLength(IntPtr hwnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int maxCount);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

		[StructLayout(LayoutKind.Sequential)]
		private struct Rect
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOZORDER = 0x0004;

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);

		[DllImport("kernel32.dll")]
		private static extern uint GetCurrentProcessId();

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr GetModuleHandle(string moduleName);

		private static string RunDialog(byte[] template, string defaultValue, IntPtr owner)
		{
			string result = null;
			IntPtr memory = Marshal.AllocHGlobal(template.Length);
			try
			{
				Marshal.Copy(template, 0, memory, template.Length);

				DialogProc proc = (hwnd, message, wParam, lParam) =>
				{
					switch (message)
					{
						case WM_INITDIALOG:
							CenterOverOwner(hwnd, owner);
							SetDlgItemText(hwnd, IdEdit, defaultValue);
							IntPtr edit = GetDlgItem(hwnd, IdEdit);
							SendMessage(edit, EM_SETSEL, IntPtr.Zero, new IntPtr(-1));
							SetForegroundWindow(hwnd);
							SetFocus(edit);
							return IntPtr.Zero;

						case WM_COMMAND:
							int id = wParam.ToInt32() & 0xFFFF;
							if (id == IdOk)
							{
								result = ReadText(GetDlgItem(hwnd, IdEdit));
								EndDialog(hwnd, new IntPtr(IdOk));
								return new IntPtr(1);
							}
							if (id == IdCancel)
							{
								EndDialog(hwnd, new IntPtr(IdCancel));
								return new IntPtr(1);
							}
							break;

						case WM_CLOSE:
							EndDialog(hwnd, new IntPtr(IdCancel));
							return new IntPtr(1);
					}
					return IntPtr.Zero;
				};

				DialogBoxIndirectParam(GetModuleHandle(null), memory, owner, proc, IntPtr.Zero);
				GC.KeepAlive(proc);
			}
			finally
			{
				Marshal.FreeHGlobal(memory);
			}
			return result;
		}

		/// <summary>
		/// Returns the window of this process that currently has the focus, so the dialog is owned by the game window.
		/// </summary>
		private static IntPtr GetGameWindow()
		{
			IntPtr window = GetForegroundWindow();
			if (window == IntPtr.Zero)
				return IntPtr.Zero;

			GetWindowThreadProcessId(window, out uint processId);
			return processId == GetCurrentProcessId() ? window : IntPtr.Zero;
		}

		private static void CenterOverOwner(IntPtr dialog, IntPtr owner)
		{
			if (owner == IntPtr.Zero || !GetWindowRect(owner, out Rect ownerRect) || !GetWindowRect(dialog, out Rect dialogRect))
				return;

			int x = ownerRect.Left + ((ownerRect.Right - ownerRect.Left) - (dialogRect.Right - dialogRect.Left)) / 2;
			int y = ownerRect.Top + ((ownerRect.Bottom - ownerRect.Top) - (dialogRect.Bottom - dialogRect.Top)) / 2;
			SetWindowPos(dialog, IntPtr.Zero, Math.Max(0, x), Math.Max(0, y), 0, 0, SWP_NOSIZE | SWP_NOZORDER);
		}

		private static string ReadText(IntPtr control)
		{
			StringBuilder text = new(GetWindowTextLength(control) + 1);
			GetWindowText(control, text, text.Capacity);
			return text.ToString();
		}

		private static byte[] BuildTemplate(string prompt, string title)
		{
			bool hasPrompt = !string.IsNullOrWhiteSpace(prompt);
			const short width = 240;
			short height = (short)(hasPrompt ? 82 : 62);
			short editTop = (short)(hasPrompt ? 30 : 10);
			short buttonTop = (short)(editTop + 26);

			using MemoryStream stream = new();
			using BinaryWriter writer = new(stream, Encoding.Unicode);

			writer.Write(WS_POPUP | WS_CAPTION | WS_SYSMENU | DS_MODALFRAME | DS_SETFONT);
			writer.Write(0u);
			writer.Write((ushort)(hasPrompt ? 4 : 3));
			writer.Write((short)0);
			writer.Write((short)0);
			writer.Write(width);
			writer.Write(height);
			writer.Write((ushort)0); // no menu
			writer.Write((ushort)0); // default dialog class
			WriteString(writer, title ?? string.Empty);
			writer.Write((ushort)9);
			WriteString(writer, "MS Shell Dlg");

			if (hasPrompt)
				WriteItem(writer, WS_CHILD | WS_VISIBLE | SS_NOPREFIX, 7, 7, 226, 20, 0xFFFF, ClassStatic, prompt);
			WriteItem(writer, WS_CHILD | WS_VISIBLE | WS_TABSTOP | WS_BORDER | ES_AUTOHSCROLL, 7, editTop, 226, 14, IdEdit, ClassEdit, string.Empty);
			WriteItem(writer, WS_CHILD | WS_VISIBLE | WS_TABSTOP | BS_DEFPUSHBUTTON, 126, buttonTop, 50, 14, IdOk, ClassButton, "OK");
			WriteItem(writer, WS_CHILD | WS_VISIBLE | WS_TABSTOP, 183, buttonTop, 50, 14, IdCancel, ClassButton, "Zrušit");

			writer.Flush();
			return stream.ToArray();
		}

		private static void WriteItem(
			BinaryWriter writer, uint style, short x, short y, short width, short height,
			int id, ushort windowClass, string text)
		{
			while (writer.BaseStream.Position % 4 != 0)
				writer.Write((byte)0);

			writer.Write(style);
			writer.Write(0u);
			writer.Write(x);
			writer.Write(y);
			writer.Write(width);
			writer.Write(height);
			writer.Write((ushort)id);
			writer.Write((ushort)0xFFFF);
			writer.Write(windowClass);
			WriteString(writer, text);
			writer.Write((ushort)0); // no creation data
		}

		private static void WriteString(BinaryWriter writer, string text)
		{
			foreach (char c in text)
				writer.Write((ushort)c);
			writer.Write((ushort)0);
		}
	}
}
