using DavyKager;

using Game.Controls;
using Game.Controls.Keyboard;
using Game.UI;

using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using UnityEngine;

namespace Game.Debug
{
	/// <summary>
	/// Records keyboard KeyDown events with inter-event delays and saves them to a text file.
	/// </summary>
	public class MacroRecorder : MonoBehaviour
	{
		public bool IsRecording { get; private set; }

		private readonly List<MacroEvent> _buffer = new();
		private Stopwatch _stopwatch;
		private long _lastTimeStamp;

		/// <summary>
		/// Starts a new recording session.
		/// </summary>
		public void StartRecording()
		{
			if (IsRecording)
				return;

			_buffer.Clear();
			_stopwatch = new Stopwatch();
			_stopwatch.Start();
			_lastTimeStamp = 0;
			IsRecording = true;

			Tolk.Speak("Nahrávám", true);
		}

		/// <summary>
		/// Stops recording and asks for file name, then saves to disk.
		/// </summary>
		public void StopAndSave()
		{
			if (!IsRecording)
				return;

			IsRecording = false;
			_stopwatch?.Stop();

			string name = WindowHandler.InputBox("Název makra (bez pøípony):", "Uložit makro", "macro1");
			if (string.IsNullOrWhiteSpace(name))
			{
				_buffer.Clear();
				_stopwatch = null;
				_lastTimeStamp = 0;
				return;
			}

			name = SanitizeFileName(name);
			try
			{
				if (!Directory.Exists(MainScript.MacroPath))
					Directory.CreateDirectory(MainScript.MacroPath);

				string path = Path.Combine(MainScript.MacroPath, name + ".txt");
				using (var writer = new StreamWriter(path, false))
				{
					writer.WriteLine("MACRO;v1");
					foreach (MacroEvent @event in _buffer)
					{
						// delayMs;D|U;KeyCode;Shift;Control;Alt
						writer.WriteLine(string.Join(";",
							@event.DelayMilliseconds.ToString(CultureInfo.InvariantCulture),
							@event.IsKeyDown ? "D" : "U",
							@event.Shortcut.Key.ToString(),
							@event.Shortcut.Shift ? "True" : "False",
							@event.Shortcut.Control ? "True" : "False",
							@event.Shortcut.Alt ? "True" : "False"
						));
					}
				}
				Tolk.Speak("Makro uloženo", true);
			}
			catch
			{
			}
			finally
			{
				_buffer.Clear();
				_stopwatch = null;
				_lastTimeStamp = 0;
			}
		}

		/// <summary>
		/// Feeds a KeyDown event (called by DebugManager).
		/// </summary>
		/// <param name="shortcut">The keyboard input to record.</param>
		public void FeedKeyDown(KeyboardInput shortcut) => Feed(shortcut, true);

		/// <summary>
		/// Feeds a KeyUp event (called by DebugManager).
		/// </summary>
		/// <param name="shortcut">The keyboard input to record.</param>
		public void FeedKeyUp(KeyboardInput shortcut) => Feed(shortcut, false);

		/// <summary>
		/// Records one macro event with computed delay since last recorded event.
		/// </summary>
		private void Feed(KeyboardInput shortcut, bool isKeyDown)
		{
			if (!IsRecording)
				return;

			if (_stopwatch == null)
			{
				_stopwatch = new Stopwatch();
				_stopwatch.Start();
				_lastTimeStamp = 0L;
			}

			long now = _stopwatch.ElapsedMilliseconds;
			int delay = (int)Math.Max(0L, now - _lastTimeStamp);
			_lastTimeStamp = now;

			MacroEvent macroEvent = new MacroEvent
			{
				DelayMilliseconds = delay,
				IsKeyDown = isKeyDown,
				Shortcut = shortcut
			};
			_buffer.Add(macroEvent);
		}

		private static string SanitizeFileName(string name)
		{
			foreach (char character in Path.GetInvalidFileNameChars())
				name = name.Replace(character, '_');
			return name.Trim();
		}
	}
}