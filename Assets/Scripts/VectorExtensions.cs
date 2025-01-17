using System.Globalization;

using UnityEngine;

namespace Game
{
	public static class VectorExtensions
	{
		public static Vector3 ToVector3(this Vector2 value, float y)
		{
			return new Vector3(value.x, y, value.y);
		}

		public static string GetString(this Vector2 vector)
		{
			string x = vector.x.Round().ToString(CultureInfo.InvariantCulture);
			string y = vector.y.Round().ToString(CultureInfo.InvariantCulture);
			return $"{x}, {y}";
		}
	}
}