using System;

namespace Luky
{
    /// <summary>
    /// Contains common mathematical functions and constants.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        private static Vector2 RadianToVector2f(float radian)
        => new Vector2((float)Math.Cos(radian), (float)Math.Sin(radian));


        public static Vector2 GetUnitVectorFromCompassDegrees(float compassDegrees)
        {
            float cartesianDegrees = (360 + 90 - compassDegrees) % 360;
            float radians = MathHelper.DegreesToRadians(cartesianDegrees);
            return RadianToVector2f(radians);
        }
        /// <summary>
        /// Correct modulo function with works properly with negative values.
        /// </summary>
        /// <param name="a">Value</param>
        /// <param name="n">Modulo range</param>
        /// <returns>Value in range 0..n</returns>
        public static float Modulo(float a, float n)
        {
            if (n == 0)
            {
                throw new DivideByZeroException(nameof(n));
            }

            float remainder = a % n; // puts a in the [-n+1, n-1] range using the remainder operator.

            /*
            if the remainder is less than zero, add n to put it in the [0, n-1] range if n is positive
            if the remainder is greater than zero, add n to put it in the [n-1, 0] range if n is negative
            */
            if ((n > 0 && remainder < 0) || (n < 0 && remainder > 0))
            {
                return remainder + n;
            }

            return remainder;
        }

        public static float InverseSqrtFast(float x)
        {
            unsafe
            {
                float xhalf = 0.5f * x;
                int i = *(int*)&x;              // Read bits as integer.
                i = 0x5f375a86 - (i >> 1);      // Make an initial guess for Newton-Raphson approximation
                x = *(float*)&i;                // Convert bits back to float
                x = x * (1.5f - xhalf * x * x); // Perform left single Newton-Raphson step.
                return x;
            }
        }

        public static float DegreesToRadians(float degrees)
        {
            const float degToRad = (float)System.Math.PI / 180.0f;
            return degrees * degToRad;
        }
    }
}