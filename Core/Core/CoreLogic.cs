using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Luky
{
    public class CoreLogic
    {
        public static CoreLogic Instance = new CoreLogic();

        /// <summary>
        /// private constructor
        /// </summary>
        private CoreLogic()
        { }

        /// <summary>
        /// 
        /// </summary>
        public void AnnounceStackTrace()
        {
            var st = new StackTrace(true);
            DebugSO.WriteDelegate(st.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="min1"></param>
        /// <param name="max1"></param>
        /// <param name="min2"></param>
        /// <param name="max2"></param>
        /// <returns></returns>
        public double ConvertValueFromRange1ToRange2(double value1, double min1, double max1, double min2, double max2)
        {
            double percent = GetPercentAtValueOfRange(value1, min1, max1);
            double value2 = GetValueAtPercentOfRange(percent, min2, max2);
            return value2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T[] EmptyArray<T>()
        =>  new T[0]; 

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
        public string GetCallerLineAndFileText()
        {
            var st = new StackTrace(true);
            var sf = st.GetFrame(2);
            return " at line " + sf.GetFileLineNumber() + " in " + sf.GetFileName();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double GetPercentAtValueOfRange(double value, double min, double max)
        {
            double percent = (value - min) / (max - min);
            return percent;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetStackTraceText()
        {
            var st = new StackTrace(true);
            return st.ToString();
        }

        // these feel more like they should go in a MyMath class, but I didn't want to bring all the methods in the MyMath class down into core.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double GetValueAtPercentOfRange(double percent, double min, double max)
        { // I felt like I kept seeing this pattern in Joseph's combat algorithms and later in AStar Epsilon.
          // So I thought I would codify it to make it more clear in my head.
          // If I want a percent of a range between 5 and 100:
          // 5 + percent * (100 - 5)
            double value = min + percent * (max - min);
            return value;
            // I guess max - min just gets us the range, then we use some modifier on it, then we add back in the min.
            // That modifier could be something that gives a different curve between 0 and 1 than just a straight percent.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool HasBitFlag(int a, int b)
          => (a | b) > 0;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public T[] MakeArray<T>(params T[] items)
        => items; 

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public List<T> MakeList<T>(T item)
        {
            var list = new List<T>(1);
            list.Add(item);
            return list;
        }

    } // cls
}