using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using UnityEngine;

namespace Game
{
	public static class VectorExtensions
	{
		private static List<float> ParseCoords(string value)
		{
			// Parse the values
			List<float> coords = new();
			string trimmed = value.Trim();

			string[] split = trimmed.Split(',');
			foreach (string coordinate in split)
				coords.Add(float.Parse(coordinate.Trim(), CultureInfo.InvariantCulture));

			// Check format
			return coords.Count is not 4 and not 2 ? throw new InvalidOperationException($"{nameof(value)} in invalid format") : coords;
		}

		public static Vector2 ToVector2(this string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException($"{nameof(value)} cann't be null.");

			// Filter out unwanted characters
			char[] charsToRemove = { '"', '{', '[', ']', '}', ')' };
			string cleanedValue = new(value.Where(c => !charsToRemove.Contains(c)).ToArray());

			List<float> coords = ParseCoords(value);
			if (coords.Count != 2)
				throw new ArgumentException("The string has to contain two float values divided by comma.");

			Vector2 result = new(coords[0].Round(), coords[1].Round());
			return result;
		}

		public static Vector3 ToVector3(this Vector2 value, float y) => new(value.x, y, value.y);

		public static string GetString(this Vector2 vector)
		{
			string x = vector.x.Round().ToString(CultureInfo.InvariantCulture);
			string y = vector.y.Round().ToString(CultureInfo.InvariantCulture);
			return $"{x}, {y}";
		}
	}
}