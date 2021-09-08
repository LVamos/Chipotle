using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Luky
{
    [Serializable]
    public abstract class DebugSO
    {
        public static string DataPath = @"Data\";
        public static readonly string MapPath = Path.Combine(DataPath, @"Map\chipotle.xml");

        public static void Say(object o)
            => SayDelegate(o.ToString());

        public static void Say(string message) => SayDelegate(message);

        public static void Say(string message, params object[] args) => SayDelegate($"{message} {FormatList(args)}.");


        public static void Say(bool and, params object[] args)
        {
            if (and)
            {
                SayDelegate(FormatList(args));
            }
            else
            {
                SayDelegate(FormatListNoAnd(args));
            }
        }



        public static Action<string> SayDelegate;
        protected static string FormatListNoAnd(params object[] args) => string.Join(", ", args);

        protected static string FormatList(params object[] args)
        {
            string[] strings = args.Select(a => a.ToString()).ToArray<string>();

            switch (strings.Length)
            {
                case 1: return strings[0];
                case 2: return strings[0] + " a " + strings[1];
                default:
                    StringBuilder message = new StringBuilder();

                    for (int i = 0; i < strings.Length - 2; i++)
                    {
                        message.Append(strings[i] + ", ");
                    }

                    message.Append(strings[args.Length - 2] + " a " + args.Last());
                    return message.ToString();
            }

        }


        public static string SoundAssetsPath;

        /// <summary>
        /// This is a static variable that makes it easy for me to output from any class. It is meant for debugging, but not meant to be used in production output.
        /// </summary>


        public const bool TestModeEnabled = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected static ArgumentException ArgumentException(string s, params object[] args)
        => new ArgumentException(String.Format(s, args));

        protected static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new Exception("Assert failed");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        protected static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                    throw new Exception(message);
            }
        }












        /// <summary>
        /// construct an Exception using String.Format. I believe the stack trace gets set when we throw it, not when we construct it.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="args"></param>
        /// <returns>new construction</returns>
        protected static Exception Exception(string s, params object[] args)
        => new Exception(String.Format(s, args));
    }
}