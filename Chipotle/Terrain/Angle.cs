using Luky;

using ProtoBuf;

using System;

using static System.Math;

namespace Game.Terrain
{
    /// <summary>
    /// Offers methods for convenient angle handling.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public struct Angle
    {
        /// <summary>
        /// Returns word description of the specified angle.
        /// </summary>
        /// <param name="degrees">The angle in compass degrees to be described</param>
        /// <returns>A word description in a string</returns>
        public static string GetDescription(double degrees)
        {
            if (degrees == 0)
                return " před tebou";
            if (degrees > 0 && degrees < 90)
                return " šikmo vpravo";
            if (degrees == 90)
                return " vpravo";
            if (degrees > 90 && degrees < 180)
                return " šikmo vpravo za tebou";
            if (degrees == 180)
                return " za tebou";
            if (degrees > 180 && degrees < 270)
                return " šikmo vlevo za tebou";
            if (degrees == 270)
                return " vlevo";
            return " šikmo vlevo";
        }


        /// <summary>
        /// Value of the angle in radians
        /// </summary>
        private double _radians;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="radians">An angle in radians</param>
        public Angle(double radians = 0)
            => _radians = NormalizeRadians(radians);

        /// <summary>
        /// The angle in cartesian degrees
        /// </summary>
        public double CartesianDegrees => RadiansToCartesianDegrees(Radians);

        /// <summary>
        /// The angle in compass degrees
        /// </summary>
        public double CompassDegrees => CartesianToCompas(CartesianDegrees);

        /// <summary>
        /// Value of the angle in radians
        /// </summary>
        public double Radians
        {
            get => _radians;
            set => _radians = NormalizeRadians(value);
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">An angle in degrees</param>
        /// <returns>The angle in radians</returns>
        public static double DegreesToRadians(double degrees)
            => degrees * PI / 180;

        /// <summary>
        /// Creates new instance of the Angle struct from a angle value in cartesian degrees.
        /// </summary>
        /// <param name="degrees">An angle in compass degrees</param>
        /// <returns>New instance of the Angle struct</returns>
        public static Angle FromCartesianDegrees(double degrees)
            => new Angle(DegreesToRadians(degrees));

        /// <summary>
        /// Creates new instance of the Angle struct from a angle value in compass degrees.
        /// </summary>
        /// <param name="degrees">An angle in compass degrees</param>
        /// <returns>New instance of the Angle struct</returns>
        public static Angle FromCompassDegrees(float degrees)
            => FromCartesianDegrees(CompassToCartesian(degrees));

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians">An angle in radians</param>
        public static implicit operator Angle(float radians)
            => new Angle(radians);

        /// <summary>
        /// Converts an angle struct to degrees.
        /// </summary>
        /// <param name="a">An angle</param>
        public static implicit operator double(Angle a)
            => a.Radians;

        /// <summary>
        /// Overloads the - operator.
        /// </summary>
        /// <param name="a1">First operand</param>
        /// <returns>Negative value of the specified angle</returns>
        public static Angle operator -(Angle a1)
            => new Angle(-a1.Radians);

        /// <summary>
        /// Overloads the - operator.
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The result of subtraction</returns>
        public static Angle operator -(Angle a, Angle b)
            => new Angle(a.Radians - b.Radians);

        /// <summary>
        /// Overloads the * operator.
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The result of multiplication</returns>
        public static Angle operator *(Angle a, Angle b)
            => new Angle(a.Radians * b.Radians);

        /// <summary>
        /// Overloads the / operator.
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>The result of division</returns>
        /// <exception cref="DivideByZeroException"></exception>
        public static Angle operator /(Angle a, Angle b)
        {
            if (b.Radians == 0)
                throw new DivideByZeroException(nameof(b.Radians));

            return new Angle(a.Radians / b.Radians);
        }

        /// <summary>
        /// Overloads the + operator.
        /// </summary>
        /// <param name="a">First operand</param>
        /// <param name="b">Second operand</param>
        /// <returns>Result of addition</returns>
        public static Angle operator +(Angle a, Angle b)
            => new Angle(a.Radians + b.Radians);

        /// <summary>
        /// Initializes an angle from east cardinal direction.
        /// </summary>
        /// <returns>New instance of Angle struct</returns>
        public Angle East()
            => FromCompassDegrees(90);

        /// <summary>
        /// Checks equality of two instances
        /// </summary>
        /// <param name="a">An object to compare</param>
        /// <returns>True if both instances are equal</returns>
        public override bool Equals(object a)
=> a != null && a.GetType() == GetType() && ((Angle)a).Radians == Radians;

        /// <summary>
        /// Converts the angle to the cardinal direction.
        /// </summary>
        /// <returns>The cardinal direction</returns>
        public CardinalDirection GetCardinalDirection()
        {
            double d = Math.Round(CompassDegrees);
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

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode()
            => Radians.GetHashCode();

        /// <summary>
        /// Initializes an angle from north cardinal direction.
        /// </summary>
        /// <returns>New instance of Angle struct</returns>
        public Angle North()
            => FromCompassDegrees(0);

        /// <summary>
        /// Initializes an angle from north east cardinal direction.
        /// </summary>
        /// <returns>New instance of Angle struct</returns>
        public Angle NorthEast()
            => FromCompassDegrees(45);

        /// <summary>
        /// Initializes an angle from north vest cardinal direction.
        /// </summary>
        /// <returns>New instance of Angle struct</returns>
        public Angle NorthWest()
            => FromCompassDegrees(315);

        /// <summary>
        /// Initializes an angle from south cardinal direction.
        /// </summary>
        /// <returns>New instance of Angle struct</returns>
        public Angle South()
            => FromCompassDegrees(180);

        /// <summary>
        /// Initializes an angle from south east cardinal direction.
        /// </summary>
        /// <returns>New instance of Angle struct</returns>
        public Angle SouthEast()
            => FromCompassDegrees(135);

        /// <summary>
        /// Initializes an angle from south vest cardinal direction.
        /// </summary>
        /// <returns>New instance of Angle struct</returns>
        public Angle SouthWest()
            => FromCompassDegrees(225);

        /// <summary>
        /// A numeric format string.
        /// </summary>
        /// <returns>The numeric format string</returns>
        public override string ToString()
            => Radians.ToString("0.00");

        /// <summary>
        /// Initializes an angle from vest cardinal direction.
        /// </summary>
        /// <returns>New instance of Angle struct</returns>
        public Angle West()
            => FromCompassDegrees(270);

        /// <summary>
        /// Converts cartesian degrees to compas degrees.
        /// </summary>
        /// <param name="cartesianDegrees">An angle in cartesian degrees</param>
        /// <returns>The angle in compass degrees</returns>
        private static double CartesianToCompas(double cartesianDegrees)
            => NormalizeDegrees(450 - cartesianDegrees);

        /// <summary>
        /// Converts compas degrees to cartesian degrees.
        /// </summary>
        /// <param name="compassDegrees">The value in compass degrees</param>
        /// <returns>The angle in cartesian degrees</returns>
        private static double CompassToCartesian(float compassDegrees)
            => NormalizeDegrees(360 + 90 - compassDegrees);

        /// <summary>
        /// Clamps value of an angle in degrees to 0..159 range.
        /// </summary>
        /// <param name="degrees">The angle value to be normalized</param>
        /// <returns>Value of the normalized angle</returns>
        private static double NormalizeDegrees(double degrees)
            => MathHelper.Modulo(degrees, 360);

        /// <summary>
        /// Clamps value of an angle in radians to 0..2*pi range.
        /// </summary>
        /// <param name="radians">The angle value to be normalized</param>
        /// <returns>Value of the normalized angle</returns>
        private static double NormalizeRadians(double radians)
            => MathHelper.Modulo(radians, 2 * (float)PI);

        /// <summary>
        /// Converts radians to cartesian degrees.
        /// </summary>
        /// <param name="radians">An angle in radians</param>
        /// <returns>The angle in cartesian degrees</returns>
        private static double RadiansToCartesianDegrees(double radians)
            => NormalizeDegrees((float)(radians * 180 / PI));
    }
}