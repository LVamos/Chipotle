using OpenTK;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Luky
{
    /// <summary>
    /// Few useful extension methods
    /// </summary>
    public static class CoreExtensionMethods
    {
        /// <summary>
        /// Adds Foreach to IEnumerable.
        /// </summary>
        /// <param name="action">The delegate to perform on each element of the IEnumerable</param>
        public static void Foreach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
                action(item);
        }

        /// <summary>
        /// Indicates whether the specified IEnumerable is null or empty.
        /// </summary>
        /// <param name="collection">The IEnumerable to check</param>
        /// <returns>True if given IEnumerable is null or empty</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
            => collection == null || !collection.Any();

        /// <summary>
        /// Converts a string to lowercase and trims spaces from edges.
        /// </summary>
        /// <param name="s">The string to modify</param>
        /// <returns>The string converted to lowercase without leading and trailing spaces</returns>
        public static string PrepareForIndexing(this string s)
=> s?.Trim(new char[] { ' ' }).ToLower();

        /// <summary>
        /// Calls predicate on each element in list and removes first matching one.
        /// </summary>
        /// <param name="list">The list to search</param>
        /// <param name="predicate">The action to perform on each element</param>
        /// <returns>Deleted element or default value</returns>
        public static T RemoveFirstOrDefault<T>(this List<T> list, Func<T, bool> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    T item = list[i];
                    list.RemoveAt(i);
                    return item;
                }
            }

            return default;
        }

        /// <summary>
        /// Converts a string to boolean
        /// </summary>
        /// <param name="s">The string to parse</param>
        /// <returns></returns>
        public static bool ToBool(this string s)
            => bool.Parse(s);

        /// <summary>
        /// Converts a string to Vector2
        /// </summary>
        /// <param name="coordinates">A string with two numbers separated by comma</param>
        public static Vector2 ToVector2(this string coordinates)
        {
            if (string.IsNullOrEmpty(coordinates))
                throw new ArgumentException(nameof(coordinates));

            string[] numbers = Regex.Split(coordinates, @", +");
            if (numbers == null || numbers.Count() != 2)
                throw new FormatException(nameof(coordinates));

            return new Vector2(float.Parse(numbers[0]), float.Parse(numbers[1]));
        }
    }
}