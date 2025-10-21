using Game.Controls;

using System;
using System.Text;

using UnityEngine;

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
			StringBuilder builder = new();
			if (Control)
				builder.Append("ctrl ");
			if (Alt)
				builder.Append("alt ");
			if (Shift)
				builder.Append("shift ");
			if (Key != KeyCode.None)
				builder.Append(Key.ToString());
			return builder.ToString();
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
			Key = KeyCode.None;

			// Split by '+' and trim whitespace
			var parts = input.Split('+', StringSplitOptions.RemoveEmptyEntries);

			foreach (var part in parts)
			{
				var token = part.Trim();

				if (token.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
					token.Equals("Control", StringComparison.OrdinalIgnoreCase))
				{
					Control = true;
				}
				else if (token.Equals("Shift", StringComparison.OrdinalIgnoreCase))
				{
					Shift = true;
				}
				else if (token.Equals("Alt", StringComparison.OrdinalIgnoreCase))
				{
					Alt = true;
				}
				else
				{
					// Try to parse as KeyCode (UnityEngine)
					if (Enum.TryParse<KeyCode>(token, true, out var parsedKey))
					{
						Key = parsedKey;
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
		public readonly KeyCode Key;

		/// <summary>
		/// Indicates whether shift key was pressed.
		/// </summary>
		public readonly bool Shift;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="modifiers">Key modifiers</param>
		/// <param name="key">The pressed key</param>
		public KeyboardInput(KeyboardModifiers modifiers, KeyCode key)
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
		public KeyboardInput(bool control, bool shift, bool alt, KeyCode key)
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
		public KeyboardInput(KeyCode key) : this(false, false, false, key)
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