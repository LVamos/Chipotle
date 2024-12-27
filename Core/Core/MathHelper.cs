using OpenTK;

using System;

using static System.Math;

namespace Luky
{
    /// <summary>
    /// A helper class with some mathematical functions
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Converts compass degrees to cartessian degrees
        /// </summary>
        /// <param name="compassDegrees">Compass degrees to convert</param>
        /// <returns>Cartessian degrees</returns>
        public static Vector2 CompassDegreesToUnitVector(float compassDegrees)
        {
            float cartesianDegrees = (360 + 90 - compassDegrees) % 360;
            float radians = DegreesToRadians(cartesianDegrees);
            return RadiansToVector2(radians);
        }

        /// <summary>
        /// Converts an angle from degrees to radians.
        /// </summary>
        /// <param name="d">Cartessian degrees to convert</param>
        /// <returns>Radians</returns>
        public static float DegreesToRadians(float d)
            => (float)(d * (PI / 180));

        /// <summary>
        /// Correct modulo function that works properly with negative values.
        /// </summary>
        /// <param name="a">Avalue to divide</param>
        /// <param name="n">Modulo range</param>
        /// <returns>Value in range 0..n</returns>
        public static double Modulo(double a, double n)
        {
            if (n == 0)
                throw new DivideByZeroException(nameof(n));

            double remainder = a % n; // puts a in the [-n+1, n-1] range using the remainder operator.

            if ((n > 0 && remainder < 0) || (n < 0 && remainder > 0))
                return remainder + n;

            return remainder;
        }

        /// <summary>
        /// Converts radians to a 2D directional vector
        /// </summary>
        /// <param name="Radians"></param>
        /// <returns>A 2D directional vector</returns>
        private static Vector2 RadiansToVector2(float radian)
        => new Vector2((float)Math.Cos(radian), (float)Math.Sin(radian));
    }
}