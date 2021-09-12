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
                action(item);
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

        public static bool ToBool(this string s)
            => bool.Parse(s);
    }
}