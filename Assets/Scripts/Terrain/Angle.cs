using ProtoBuf;

using System;

using UnityEngine;

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
		/// Gets the unit vector of the current Vector2.
		/// </summary>
		public Vector2 UnitVector => RadiansToVector2(Radians).normalized;

		/// <summary>
		/// Converts the given angle in radians to a 2D vector.
		/// </summary>
		private Vector2 RadiansToVector2(double radians)
			=> new((float)Cos(radians), (float)Sin(radians));

		/// <summary>
		/// Correct modulo function that works properly with negative values.
		/// </summary>
		/// <param name="a">Avalue to divide</param>
		/// <param name="n">Modulo range</param>
		/// <returns>Value in range 0..n</returns>
		private static double Modulo(double a, double n)
		{
			if (n == 0)
				throw new DivideByZeroException(nameof(n));

			double remainder = a % n; // puts a in the [-n+1, n-1] range using the remainder operator.

			if ((n > 0 && remainder < 0) || (n < 0 && remainder > 0))
				return remainder + n;

			return remainder;
		}

		/// <summary>
		/// Returns word description of the specified angle.
		/// </summary>
		/// <param name="degrees">The angle in compass degrees to be described</param>
		/// <returns>A word description in a string</returns>
		public static string GetRelativeDirection(double degrees, float distance)
		{
			if (distance < 3)
			{
				if (degrees == 0)
					return " před tebou";
				if (degrees is > 0 and < 90)
					return " šikmo vpravo";
				if (degrees == 90)
					return " vpravo";
				if (degrees is > 90 and < 180)
					return " šikmo vpravo za tebou";
				if (degrees == 180)
					return " za tebou";
				if (degrees is > 180 and < 270)
					return " šikmo vlevo za tebou";
				if (degrees == 270)
					return " vlevo";
				return " šikmo vlevo";
			}

			// For longer distance describe the direction less accurately.
			if (degrees is > 325 or <= 35)
				return " před tebou";
			if (degrees is > 35 and <= 55)
				return " šikmo vpravo";
			if (degrees is > 55 and <= 125)
				return " vpravo";
			if (degrees is > 125 and <= 145)
				return " šikmo vpravo za tebou";
			if (degrees is > 145 and <= 215)
				return " za tebou";
			if (degrees is > 215 and <= 235)
				return " šikmo vlevo za tebou";
			if (degrees is > 235 and <= 305)
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
			=> new(DegreesToRadians(degrees));

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
			=> new(radians);

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
			=> new(-a1.Radians);

		/// <summary>
		/// Overloads the - operator.
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <returns>The result of subtraction</returns>
		public static Angle operator -(Angle a, Angle b)
			=> new(a.Radians - b.Radians);

		/// <summary>
		/// Overloads the * operator.
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <returns>The result of multiplication</returns>
		public static Angle operator *(Angle a, Angle b)
			=> new(a.Radians * b.Radians);

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

			return new(a.Radians / b.Radians);
		}

		/// <summary>
		/// Overloads the + operator.
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <returns>Result of addition</returns>
		public static Angle operator +(Angle a, Angle b)
			=> new(a.Radians + b.Radians);

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
			=> Modulo(degrees, 360);

		/// <summary>
		/// Clamps value of an angle in radians to 0..2*pi range.
		/// </summary>
		/// <param name="radians">The angle value to be normalized</param>
		/// <returns>Value of the normalized angle</returns>
		private static double NormalizeRadians(double radians)
			=> Modulo(radians, 2 * (float)PI);

		/// <summary>
		/// Converts radians to cartesian degrees.
		/// </summary>
		/// <param name="radians">An angle in radians</param>
		/// <returns>The angle in cartesian degrees</returns>
		private static double RadiansToCartesianDegrees(double radians)
			=> NormalizeDegrees((float)(radians * 180 / PI));

		public static string GetClockDirection(float degrees)
		{
			int clockDirection = (int)Math.Round((degrees % 360) / 30);
			clockDirection = clockDirection % 12;
			string description = clockDirection switch
			{
				0 => "na dvanácti hodinách",
				1 => "na jedné hodině",
				2 => "na dvou hodinách",
				3 => "na třech hodinách",
				4 => "na čtyřech hodinách",
				5 => "na pěti hodinách",
				6 => "na šesti hodinách",
				7 => "na sedmi hodinách",
				8 => "na osmi hodinách",
				9 => "na devíti hodinách",
				10 => "na deseti hodinách",
				11 => "na jedenácti hodinách",
				_ => throw new ArgumentOutOfRangeException(nameof(degrees), degrees, null)
			};
			return description;
		}
	}
}