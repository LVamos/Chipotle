using System;
using System.Collections.Generic;

using UnityEngine.InputSystem;

namespace Game.Controls.Keyboard
{
	/// <summary>
	/// Represents a keyboard shortcut.
	/// </summary>
	[Serializable]
	public struct KeyboardInput
	{
		public override string ToString()
		{
			List<string> segments = new();
			if (Control)
				segments.Add("Control");
			if (Shift)
				segments.Add("Shift");
			if (Alt)
				segments.Add("Alt");
			if (Key != Key.None)
				segments.Add(Key.ToString());
			return string.Join("+", segments);
		}

		/// <summary>
		/// Constructor that parses a keyboard shortcut from a string (e.g. "Ctrl+Alt+F1" or "Shift+S").
		/// </summary>
		/// <param name="input">String representation of the shortcut</param>
		/// <exception cref="ArgumentException">Thrown if the key name is invalid</exception>
		public KeyboardInput(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				throw new ArgumentException("KeyboardInput string cannot be null or empty.", nameof(input));

			Control = false;
			Shift = false;
			Alt = false;
			Key = Key.None;
			bool keyFound = false;

			// Split by '+' and trim whitespace
			string[] parts = input.Split('+', StringSplitOptions.RemoveEmptyEntries);

			foreach (string part in parts)
			{
				string token = part.Trim();

				if (token.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
					token.Equals("Control", StringComparison.OrdinalIgnoreCase))
					Control = true;
				else if (token.Equals("Shift", StringComparison.OrdinalIgnoreCase))
					Shift = true;
				else if (token.Equals("Alt", StringComparison.OrdinalIgnoreCase))
					Alt = true;
				else
				{
					// Try to parse as Key (UnityEngine)
					if (Enum.TryParse<Key>(token, true, out Key parsedKey))
					{
						if (keyFound)
							throw new ArgumentException($"Multiple keys specified in '{input}'");

						Key = parsedKey;
						keyFound = true;
					}
					else
					{
						throw new ArgumentException($"Invalid key name: '{token}' in '{input}'");
					}
				}
			}
		}

		/// <summary>
		/// Indicates whether alt key was pressed.
		/// </summary>
		public readonly bool Alt;

		/// <summary>
		/// Indicates whether control key was pressed.
		/// </summary>
		public readonly bool Control;

		/// <summary>
		/// Value of pressed keys.
		/// </summary>
		public readonly Key Key;

		/// <summary>
		/// Indicates whether shift key was pressed.
		/// </summary>
		public readonly bool Shift;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="modifiers">Key modifiers</param>
		/// <param name="key">The pressed key</param>
		public KeyboardInput(KeyboardModifiers modifiers, Key key)
		{
			Control = modifiers.HasFlag(KeyboardModifiers.Control);
			Alt = modifiers.HasFlag(KeyboardModifiers.Alt);
			Shift = modifiers.HasFlag(KeyboardModifiers.Shift);
			Key = key;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="control">Specifies if the control key was pressed</param>
		/// <param name="shift">Specifies if the shift key was pressed</param>
		/// <param name="alt">Specifies if the alt key was pressed</param>
		/// <param name="key">Value of pressed keys</param>
		public KeyboardInput(bool control, bool shift, bool alt, Key key)
		{
			Control = control;
			Shift = shift;
			Alt = alt;
			Key = key;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="key">Value of pressed key</param>
		public KeyboardInput(Key key) : this(false, false, false, key)
		{
		}

		/// <summary>
		/// Overloads the != operator.
		/// </summary>
		/// <param name="k1">First operand</param>
		/// <param name="k2">second operand</param>
		/// <returns>True if the operands are inequal</returns>
		public static bool operator !=(KeyboardInput k1, KeyboardInput k2)
			=> !(k1 == k2);

		/// <summary>
		/// Overloads the == operator.
		/// </summary>
		/// <param name="k1">First operand</param>
		/// <param name="k2">second operand</param>
		/// <returns>True if the operands are equal</returns>
		public static bool operator ==(KeyboardInput k1, KeyboardInput k2)
			=> (k1.Control == k2.Control && k1.Shift == k2.Shift && k1.Alt == k2.Alt && k1.Key == k2.Key);

		/// <summary>
		/// Checks if two instances are equal.
		/// </summary>
		/// <param name="obj">Another object to be checked</param>
		/// <returns>True if both instances are equal</returns>
		public override bool Equals(System.Object obj)
			=> (obj is KeyboardInput shortcut && this == shortcut);

		/// <summary>
		/// Returns a hash code for the instance.
		/// </summary>
		/// <returns>a hash code for the instance</returns>
		public override int GetHashCode()
			=> Key.GetHashCode();
	}
}