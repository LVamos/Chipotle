using System;
using System.Diagnostics;

namespace Luky
{
    public static class GlobalTime
    {
        private static Stopwatch _stopwatch  = Stopwatch.StartNew();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ulong Get()
        => (ulong)Math.Floor(_stopwatch .Elapsed.TotalMilliseconds); 
    } // cls
}