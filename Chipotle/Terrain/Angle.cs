using Luky;

using System;

using static System.Math;


namespace Game.Terrain
{
    public struct Angle
    {

        public CardinalDirection GetCardinalDirection()
        {
            float d = CompassDegrees;
            if (d >= 0 && d < 45)
                return CardinalDirection.North;

            if (d >= 45 && d < 90)
                return CardinalDirection.NorthEast;

            if (d >= 90 && d < 135)
                return CardinalDirection.East;

            if (d >= 135 && d < 180)
                return CardinalDirection.SouthEast;

            if (d >= 180 && d < 225)
                return CardinalDirection.South;

            if (d >= 225 && d < 270)
                return CardinalDirection.SouthWest;

            if (d >= 270 && d < 315)
                return CardinalDirection.West;

            return CardinalDirection.NorthWest;
        }

        private float _radians;
        public float Radians
        {
            get => _radians;
            set => _radians = NormalizeRadians(value);
        }

        public static implicit operator Angle(float radians)
            => new Angle(radians);

        public static implicit operator float(Angle a)
            => a.Radians;

        public static Angle operator -(Angle a1)
            => new Angle(-a1.Radians);

        public static Angle operator +(Angle a, Angle b)
            => new Angle(a.Radians + b.Radians);

        public static Angle operator -(Angle a, Angle b)
            => new Angle(a.Radians - b.Radians);

        public static Angle operator *(Angle a, Angle b)
            => new Angle(a.Radians * b.Radians);

        public static Angle operator /(Angle a, Angle b)
        {
            if (b.Radians == 0)
                throw new DivideByZeroException(nameof(b.Radians));

            return new Angle(a.Radians / b.Radians);
        }

        public override bool Equals(object a)
=> a != null && a.GetType() == GetType() && ((Angle)a).Radians == Radians;


        public override int GetHashCode()
            => Radians.GetHashCode();

        public override string ToString()
            => Radians.ToString("0.00");

        public Angle(float radians = 0)
            => _radians = NormalizeRadians(radians);

        private static float CompassToCartesian(float compassDegrees)
            => NormalizeDegrees(360 + 90 - compassDegrees);

        private static float CartesianToCompas(float cartesianDegrees)
            => NormalizeDegrees(450 - cartesianDegrees);

        public float CartesianDegrees => RadiansToCartesianDegrees(Radians);


        public Angle North()
            => FromCompassDegrees(0);

        public Angle NorthEast()
            => FromCompassDegrees(45);

        public Angle East()
            => FromCompassDegrees(90);

        public Angle SouthEast()
            => FromCompassDegrees(135);

        public Angle South()
            => FromCompassDegrees(180);

        public Angle SouthWest()
            => FromCompassDegrees(225);

        public Angle West()
            => FromCompassDegrees(270);

        public Angle NorthWest()
            => FromCompassDegrees(315);



        public float CompassDegrees => CartesianToCompas(CartesianDegrees);

        private static float RadiansToCartesianDegrees(float radians)
            => NormalizeDegrees((float)(radians * 180 / PI));


        private static float NormalizeDegrees(float degrees)
            => MathHelper.Modulo(degrees, 360);

        private static float NormalizeRadians(float radians)
            => radians;//MathHelper.Modulo(radians, 2 * (float)PI);

        public static Angle FromCompassDegrees(float degrees)
            => FromCartesianDegrees(CompassToCartesian(degrees));

        public static Angle FromCartesianDegrees(float degrees)
            => new Angle(DegreesToRadians(degrees));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float DegreesToRadians(float degrees)
            => degrees * (float)PI / 180;


    }
}
