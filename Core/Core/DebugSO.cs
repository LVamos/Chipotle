using System;

namespace Luky
{
    /// <summary>
    /// Base class for all solution with test methods
    /// </summary>
    [Serializable]
    public abstract class DebugSO
    {
        /// <summary>
        /// Checks for a condition; if the condition is false, throws an Exception.
        /// </summary>
        /// <param name="condition">The condition to test</param>
        /// <param name="message">Message for the potential exception</param>
        /// <exception cref="Exception"></exception>
        protected static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new Exception(message);
        }
    }
}