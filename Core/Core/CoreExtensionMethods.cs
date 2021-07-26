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




        public static string PrepareForIndexing(this string name)
    => name?.Trim(new char[] { ' ' }).ToLower();


        private static readonly char[] _onSpace = new char[] { ' ' };

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static T AddAndReturn<T>(this List<T> list, T item)
        {
            list.Add(item);
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string AllButFirstSplit(this string s, char seperator)
        {
            string[] items = s.Split(seperator);

            if (items.Length == 1)
            {
                return "";
            }

            return String.Join(seperator.ToString(), items, 1, items.Length - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string AllButLastSplit(this string s, char seperator)
        {
            string[] items = s.Split(seperator);

            if (items.Length == 1)
            {
                return "";
            }

            return String.Join(seperator.ToString(), items, 0, items.Length - 1);
        }

        /// <summary>
        /// Checks if two float values are nearly equal.
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns>true if given values differ just 0.0000001 or less.</returns>
        public static bool AlmostEquals(this float f1, float f2)
        => Math.Abs(f1 - f2) <= .00001;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="text"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static StringBuilder AppendLine(this StringBuilder sb, string text, params object[] args)
        => sb.AppendFormat(text, args).AppendLine();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 AsV3(this Vector2 v)
        => new Vector3(v.X, v.Y, 0);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool ContainsAll<T>(this List<T> list1, List<T> list2)
        {
            foreach (T obj in list2)
            {
                if (!list1.Contains(obj))
                {
                    return false;
                }
            } // end foreach object in list2
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static bool ContainsAny<T>(this List<T> list1, List<T> list2)
        {
            foreach (T obj in list2)
            {
                if (list1.Contains(obj))
                {
                    return true;
                }
            } // end foreach object in list2
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool EndsWith(this StringBuilder sb, char c)
        => sb.Length > 0 && sb[sb.Length - 1] == c;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string EnsureBackSlashAtEnd(this string s)
        {
            if (!s.EndsWith("\\"))
            {
                return s + "\\";
            }

            return s;
        }

        // Linq extensions

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="exceptions"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> source, params TSource[] exceptions)
        => source.Except(exceptions.AsEnumerable());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string FirstSplit(this string s, char seperator)
        {
            string[] items = s.Split(seperator);
            return items[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void Foreach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            dict.Add(key, default(TValue));
            return dict[key];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetOrAddNew<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            dict.Add(key, new TValue());
            return dict[key];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsBlank(this string s)
        => String.IsNullOrEmpty(s);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static bool IsBlank(this StringBuilder sb)
        => sb.Length == 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsDigit(this char c)
           => 48 <= c && c <= 57; // ascii digits 0 through 9 map to integer values 48 through 57 respectively

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this List<T> list)
        => list == null || list.Count == 0;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this T[] list)
        => list == null || list.Length == 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsLetter(this char c)
           // upper case letters map to integer values 65 through 90, lower case letters map to integer values 97 through 122
           => (65 <= c && c <= 90) || (97 <= c && c <= 122);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNotEmpty<T>(this List<T> list)
        => list != null && list.Count > 0;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNotEmpty<T>(this T[] list)
        => list != null && list.Length > 0;

        // we hide methods that work on T or object from intellisense because they clutter every object.
        // but apparently this doesn't work.
        //[EditorBrowsable(EditorBrowsableState.Never)]
        // I decided to move the In method to CoreLogic so it doesn't pollute intellisense for every object.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsSet(this string s)
        => !String.IsNullOrEmpty(s);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsValidIndexFor(this int index, ICollection list)
       => 0 <= index && index < list.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Last<T>(this List<T> list)
        => list[list.Count - 1];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static char Last(this string s)
        => s[s.Length - 1];

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T LastOrDefault<T>(this List<T> list)
        {
            if (list.Count == 0)
            {
                return default(T);
            }

            return list[list.Count - 1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static string LastSplit(this string s, char seperator)
        {
            string[] items = s.Split(seperator);
            return items[items.Length - 1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Pop<T>(this List<T> list)
        {
            int index = list.Count - 1;
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        // Stream extensions

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string[] ReadAllLines(this Stream s)
        {
            List<string> list = new List<string>();
            using (StreamReader sr = new StreamReader(s))
            {
                while (!sr.EndOfStream)
                {
                    list.Add(sr.ReadLine());
                }
            } // end using stream reader
            return list.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ReadAllText(this Stream s)
        {
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
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

        // a couple nice methods for rounding floats and doubles

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static float Round(this float f) =>
            // (if a decimal string with at most 6 significant decimal is converted to IEEE 754 single precision and then converted back to the same number of significant decimal, then the final string should match the original;
            // taken from: http://en.wikipedia.org/wiki/Single_precision
            (float)Math.Round(f, 6);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static double Round(this double d)
            // If a decimal string with at most 15 significant digits is converted to IEEE 754 double precision representation and then converted back to a string with the same number of significant digits, then the final string should match the original;
            // taken from: http://en.wikipedia.org/wiki/Double-precision_floating-point_format
            => Math.Round(d, 6);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int RoundToInt(this double d)
            => (int)Math.Round(d);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int RoundToInt(this float f)
            => (int)Math.Round(f);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string[] SplitOnSpace(this string s)
        => s.Split(_onSpace, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T[] Tail<T>(this T[] source)
           // The name tail is taken from functional languages
           => source.Slice(1, source.Length);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool ToBool(this int i)
          => i != 0;

        public static bool ToBool(this string s)
            => bool.Parse(s);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sw"></param>
        public static void WL(this StreamWriter sw)
        => sw.WriteLine();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="s"></param>
        public static void WL(this StreamWriter sw, string s)
        => sw.WriteLine(s);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="o"></param>
        public static void WL(this StreamWriter sw, object o)
        => sw.WriteLine(o.ToString());
    } // cls
}