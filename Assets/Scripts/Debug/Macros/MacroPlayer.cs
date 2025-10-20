using DavyKager;

using Game;
using Game.Controls;
using Game.Controls.Keyboard;
using Game.Messaging;
using Game.Messaging.Events.Input;
using Game.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using UnityEngine;

namespace Game.Debug
{
	/// <summary>
	/// Plays a recorded macro by sending KeyDown, KeyUp and KeyPress events with preserved delays.
	/// </summary>
	public class MacroPlayer : MonoBehaviour
	{

		public bool IsPlaying { get; private set; }

		// Sender used for KeyPressed messages (DebugManager recommended)
		public MessagingObject Sender { get; set; }

		private Coroutine _playCoroutine;

		/// <summary>
		/// Plays macro by name (without extension). Returns false if it cannot start.
		/// </summary>
		public bool Play(string name)
		{
			if (IsPlaying)
				return false;

			if (string.IsNullOrWhiteSpace(name))
				return false;

			string path = Path.Combine(MainScript.MacroPath, name + ".txt");
			if (!File.Exists(path))
			{
				Tolk.Speak("Makro nenalezeno", true);
				return false;
			}

			List<MacroEvent> events = Load(path);
			if (events == null || events.Count == 0)
				return false;

			_playCoroutine = StartCoroutine(PlayCoroutine(events));
			return true;
		}

		/// <summary>
		/// Stops playback if running.
		/// </summary>
		public void Stop()
		{
			if (!IsPlaying)
				return;

			if (_playCoroutine != null)
				StopCoroutine(_playCoroutine);

			IsPlaying = false;
			_playCoroutine = null;
		}

		private List<MacroEvent> Load(string path)
		{
			try
			{
				List<MacroEvent> events = new();
				string[] lines = File.ReadAllLines(path);
				int version = 1;

				foreach (string line in lines)
				{
					if (line.StartsWith("MACRO"))
					{
						string[] versionParts = line.Split(new[] { ';' }, StringSplitOptions.None);
						if (versionParts.Length > 1 && versionParts[1].StartsWith("v"))
							int.TryParse(versionParts[1].Substring(1), out version);
						continue;
					}

					string[] parts = line.Split(new[] { ';' }, StringSplitOptions.None);
					int delay = int.Parse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture);

					if (version == 1)
					{
						// Old format: delayMs;D|U;KeyCode;Shift;Control;Alt
						bool isKeyDown = false;
						int offset = 0;
						if (parts.Length == 6)
						{
							isKeyDown = string.Equals(parts[1], "D", StringComparison.OrdinalIgnoreCase);
							offset = 1;
						}

						string keyString = parts[1 + offset];
						bool shift = bool.Parse(parts[2 + offset]);
						bool control = bool.Parse(parts[3 + offset]);
						bool alt = bool.Parse(parts[4 + offset]);

						KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), keyString, true);
						KeyboardInput shortcut = new KeyboardInput(control, shift, alt, key);

						MacroEvent @event = new MacroEvent
						{
							DelayMilliseconds = Math.Max(0, delay),
							EventType = isKeyDown ? MacroEventType.KeyDown : MacroEventType.KeyUp,
							Shortcut = shortcut
						};
						events.Add(@event);
					}
					else
					{
						// New format: delayMs;D|U|P;KeyCode|Char;Shift;Control;Alt
						MacroEventType eventType = parts[1].ToUpperInvariant() switch
						{
							"D" => MacroEventType.KeyDown,
							"U" => MacroEventType.KeyUp,
							"P" => MacroEventType.KeyPress,
							_ => MacroEventType.KeyDown
						};

						string keyOrChar = parts[2];
						bool shift = bool.Parse(parts[3]);
						bool control = bool.Parse(parts[4]);
						bool alt = bool.Parse(parts[5]);

						MacroEvent @event = new MacroEvent
						{
							DelayMilliseconds = Math.Max(0, delay),
							EventType = eventType
						};

						if (eventType == MacroEventType.KeyPress)
						{
							@event.Character = keyOrChar.Length > 0 ? keyOrChar[0] : '\0';
							@event.Shortcut = new KeyboardInput(KeyCode.None);
						}
						else
						{
							KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), keyOrChar, true);
							@event.Shortcut = new KeyboardInput(control, shift, alt, key);
						}

						events.Add(@event);
					}
				}
				return events;
			}
			catch
			{
				Tolk.Speak("Chyba pøi naèítání makra", true);
				return null;
			}
		}

		private IEnumerator PlayCoroutine(List<MacroEvent> events)
		{
			IsPlaying = true;
			Tolk.Speak("Pøehrávám", true);
			foreach (MacroEvent @event in events)
			{
				if (@event.DelayMilliseconds > 0)
					yield return new WaitForSeconds(@event.DelayMilliseconds / 1000f);

				try
				{
					switch (@event.EventType)
					{
						case MacroEventType.KeyDown:
							WindowHandler.OnKeyDown(@event.Shortcut);
							break;
						case MacroEventType.KeyUp:
							WindowHandler.OnKeyUp(@event.Shortcut);
							break;
						case MacroEventType.KeyPress:
							WindowHandler.OnKeyPress(@event.Character);
							break;
					}
				}
				catch { }
			}

			Tolk.Speak("Pøehrávání dokonèeno", true);
			IsPlaying = false;
			_playCoroutine = null;
		}
	}
}