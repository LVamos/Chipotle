using UnityEngine;

namespace Game
{
	public static class CharExtensions
	{
		/// <summary>
		/// Converts a character to its corresponding Unity KeyCode.
		/// </summary>
		/// <param name="letter">The character to convert.</param>
		/// <returns>The corresponding KeyCode, or KeyCode.None if no mapping exists.</returns>
		public static KeyCode ToKeyCode(this char letter)
		{
			// Convert to uppercase for consistent mapping
			char upperChar = char.ToUpper(letter);

			// Alphanumeric keys
			if (upperChar >= 'A' && upperChar <= 'Z')
				return KeyCode.A + (upperChar - 'A');

			if (letter >= '0' && letter <= '9')
				return KeyCode.Alpha0 + (letter - '0');

			// Special characters
			return letter switch
			{
				' ' => KeyCode.Space,
				'.' => KeyCode.Period,
				',' => KeyCode.Comma,
				';' => KeyCode.Semicolon,
				':' => KeyCode.Colon,
				'\'' => KeyCode.Quote,
				'"' => KeyCode.DoubleQuote,
				'/' => KeyCode.Slash,
				'\\' => KeyCode.Backslash,
				'[' => KeyCode.LeftBracket,
				']' => KeyCode.RightBracket,
				'-' => KeyCode.Minus,
				'=' => KeyCode.Equals,
				'+' => KeyCode.Plus,
				'*' => KeyCode.Asterisk,
				'&' => KeyCode.Ampersand,
				'@' => KeyCode.At,
				'#' => KeyCode.Hash,
				'$' => KeyCode.Dollar,
				'%' => KeyCode.Percent,
				'^' => KeyCode.Caret,
				'!' => KeyCode.Exclaim,
				'?' => KeyCode.Question,
				'(' => KeyCode.LeftParen,
				')' => KeyCode.RightParen,
				'<' => KeyCode.Less,
				'>' => KeyCode.Greater,
				'`' => KeyCode.BackQuote,
				'_' => KeyCode.Underscore,
				'|' => KeyCode.Pipe,
				_ => KeyCode.None
			};
		}

	}
}
