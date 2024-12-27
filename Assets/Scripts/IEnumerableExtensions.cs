using System.Collections.Generic;
using System.Linq;

namespace Game
{
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Indicates whether the specified IEnumerable is null or empty.
		/// </summary>
		/// <param name="collection">The IEnumerable to check</param>
		/// <returns>True if given IEnumerable is null or empty</returns>
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
			=> collection == null || !collection.Any();

	}
}