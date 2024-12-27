using System;

namespace Game
{
	public static class FloatExtensions
	{
		public static float Round(this float value) => (float)Math.Round(value, 1, MidpointRounding.AwayFromZero);
	}
}