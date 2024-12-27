namespace Game
{
	public static class BoolExtensions
	{
		/// <summary>
		/// Converts a string to boolean
		/// </summary>
		/// <param name="s">The string to parse</param>
		/// <returns></returns>
		public static bool ToBool(this string s)
			=> bool.Parse(s);

	}
}