using System.Collections.Generic;
using System.Linq;

namespace Game.Debug
{
	public static class DebugUtils
	{
		/// <summary>
		/// Converts any IEnumerable into a readable string for logging.
		/// </summary>
		public static string JoinEnumerable<T>(this IEnumerable<T> source, string separator = ", ")
		{
			if (source == null)
				return "null";

			try
			{
				return string.Join(separator, source.Select(x => x?.ToString() ?? "null"));
			}
			catch
			{
				return "[error enumerating]";
			}
		}
	}
}
