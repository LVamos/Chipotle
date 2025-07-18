﻿using Game.Terrain;

using System;

namespace Game
{
	public static class StringExtensions
	{
		public static Zone.ZoneType ToZoneType(this string value)
		{
			if (string.Equals(value, "outdoor", StringComparison.OrdinalIgnoreCase))
				return Zone.ZoneType.Outdoor;
			else if (string.Equals(value, "indoor", StringComparison.OrdinalIgnoreCase))
				return Zone.ZoneType.Indoor;
			throw new ArgumentException(nameof(value));
		}
		/// <summary>
		/// Converts a string to lowercase and trims spaces from edges.
		/// </summary>
		/// <param name="s">The string to modify</param>
		/// <returns>The string converted to lowercase without leading and trailing spaces</returns>
		public static string PrepareForIndexing(this string s)
			=> s?.Trim(new char[] { ' ' }).ToLower();
	}
}