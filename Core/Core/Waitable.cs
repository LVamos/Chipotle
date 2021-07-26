using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Waitable
    {
        public int? Result;
        public ulong? Timeout;

        // optionally set to a timeout in milliseconds. This is an absolute time, not a remaining time, which allows Waitable.WaitAny to accurately calculate remaining time even when time has already elapsed.
        public List<WaitHandle> WaitHandles = new List<WaitHandle>();

        public bool WorkImmediatelyAvailable; // set by the Waitable owner if there is more work to do without waiting for a timeout or a WaitHandle.
                                              // set by WaitAny, and cleared by the Waitable owner, null means not triggered, -1 means timeout triggered, -2 means WorkImmediatelyAvailable, anything else is the wait handle index.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="waitables"></param>
        /// <returns></returns>
        public static int WaitAny(Waitable[] waitables)
        { // returns the index of the waitable that can now perform work, and sets the result field of that waitable.
          // first check for waitables that already have work available
            for (int i = 0; i < waitables.Length; i++)
            {
                if (waitables[i].WorkImmediatelyAvailable)
                {
                    waitables[i].Result = -2;
                    return i;
                }
            } // end foreach waitable

            // find the shortest timeout and remember which waitable index it is associated with.
            int timeoutIndex = -1;
            ulong endTime = ulong.MaxValue;
            for (int i = 0; i < waitables.Length; i++)
            {
                if (waitables[i].Timeout.HasValue && waitables[i].Timeout.Value < endTime)
                {
                    timeoutIndex = i;
                    endTime = waitables[i].Timeout.Value;
                }
            } // end foreach waitable

            ulong currentTime = GlobalTime.Get();
            int timeout = System.Threading.Timeout.Infinite;
            if (timeoutIndex != -1)
            {
                timeout = (int)(endTime - currentTime);
                if (timeout <= 0)
                { // the timeout expired
                    waitables[timeoutIndex].Result = -1;
                    return timeoutIndex;
                }
            }
            WaitHandle[] handles = waitables.SelectMany(p => p.WaitHandles).ToArray();
            int index = WaitHandle.WaitAny(handles, timeout);

            if (index == WaitHandle.WaitTimeout)
            { // the timeout expired
                waitables[timeoutIndex].Result = -1;
                return timeoutIndex;
            }

            // otherwise one of the WaitHandles triggered.
            int handlesSoFar = 0;
            for (int i = 0; i < waitables.Length; i++)
            {
                if (index < handlesSoFar + waitables[i].WaitHandles.Count)
                {
                    waitables[i].Result = index - handlesSoFar;
                    return i;
                }
            } // end foreach waitable
            throw new Exception("Should not get here");
        }
    }
}