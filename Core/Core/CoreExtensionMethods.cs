using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    public static class CoreExtensionMethods
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
    => collection == null || !collection.Any();


        public static string PrepareForIndexing(this string name)
    => name?.Trim(new char[] { ' ' }).ToLower();

        public static void Foreach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                action(item);
            }
        }

        public static bool IsNotEmpty<T>(this List<T> list)
        => list != null && list.Count > 0;

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
            } // end for loop

            return default(T);
        }
        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start index, exclusive for end index.
        /// </summary>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends.
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;

            // Return new array.
            T[] res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }

        public static bool ToBool(this string s)
            => bool.Parse(s);
    }
}