using System;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class MyMath : DebugSO
    {



        /// <summary>
        /// 
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="interest"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static double CompoundInterest(double principal, double interest, int iterations)
           => principal * Math.Pow(1 + interest, iterations);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="planeStart"></param>
        /// <param name="planeEnd"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Vector3 GetNearestPointOnPlane(Vector3 planeStart, Vector3 planeEnd, Vector3 point)
        {
            // taken from http://stackoverflow.com/questions/9368436/3d-perpendicular-point-on-line-from-3d-point
            // hmm, reviewing this later I realize the article was about 3D lines, not planes at all.
            Vector3 p1 = planeStart;
            Vector3 p2 = planeEnd;
            Vector3 q = point;

            Vector3 u = p2 - p1;
            Vector3 pq = q - p1;
            Vector3 w2 = pq - Vector3.Multiply(u, Vector3.Dot(pq, u) / u.LengthSquared);

            return q - w2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compassDegrees"></param>
        /// <returns></returns>
        public static Vector2 GetUnitVectorFromCompassDegrees(float compassDegrees)
        {
            float cartesianDegrees = (360 + 90 - compassDegrees) % 360;
            float radians = MathHelper.DegreesToRadians(cartesianDegrees);
            return RadianToVector2f(radians);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static Vector2 OldGetUnitVectorFromCompassDegrees(float degrees)
        {
            float angleInRadians = MathHelper.DegreesToRadians(degrees);
            //angleInRadians -= (float)Math.PI; // move it from 0 to 2PI to negative PI to positive PI.

            // with my new, slightly better understanding of sine and cosine I think these are in reverse.
            // but that probably works because I'm doing 360 degrees with north as 0 degrees.
            // Also note I multiply Y by negative 1.
            float headingX = (float)Math.Sin(angleInRadians);
            float headingY = -(float)Math.Cos(angleInRadians);
            Vector2 vec = new Vector2(headingX, headingY);
            // not sure if it was already normalized, but did it for good measure
            vec.Normalize();
            return vec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static float RadiansToDegrees(float radians)
        {
            float degrees = (float)(radians * 180 / Math.PI);
            return degrees;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static double RadiansToDegrees(double radians)
        {
            double degrees = radians * 180 / Math.PI;
            return degrees;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        private static Vector2 RadianToVector2f(float radian)
        => new Vector2((float)Math.Cos(radian), (float)Math.Sin(radian));
    } // cls
}