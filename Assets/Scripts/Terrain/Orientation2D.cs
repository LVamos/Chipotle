using ProtoBuf;

using System;
using System.Collections.Generic;

using UnityEngine;

using static System.Math;

namespace Game.Terrain
{
	/// <summary>
	/// Represents orientation of an NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public struct Orientation2D
	{
		/// <summary>
		///  The orientation in compass-like degrees
		/// </summary>
		public int CompassDegrees => _compassDegrees;

		/// <summary>
		/// A directional unit vecotr defining the orientation
		/// </summary>
		private Vector2 _unitVector;
		private int _compassDegrees;
		private Angle _angle;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="x">The X component of the vector defining the orientation</param>
		/// <param name="y">The Y component of the vector defining the orientation</param>
		public Orientation2D(float x, float y) : this(new(x, y))
		{ }
		/// <summary>
		/// Clamps value of an angle in degrees to 0..159 range.
		/// </summary>
		/// <param name="degrees">The angle value to be normalized</param>
		/// <returns>Value of the normalized angle</returns>
		private static double NormalizeDegrees(double degrees)
			=> Modulo(degrees, 360);

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
		/// Converts radians to cartesian degrees.
		/// </summary>
		/// <param name="radians">An angle in radians</param>
		/// <returns>The angle in cartesian degrees</returns>
		private static double RadiansToDegrees(double radians)
			=> NormalizeDegrees((radians * 180 / PI));

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="unitVector">A directional unit vector defining the orientation</param>
		public Orientation2D(Vector2 unitVector)
		{
			Chipotle = false;
			_unitVector = unitVector;
			_unitVector.Normalize();
			_compassDegrees = (int)RadiansToDegrees(Math.Atan2(unitVector.y, unitVector.x));
			_compassDegrees = (_compassDegrees + 360) % 360; // Zajištění, aby byl úhel v rozmezí 0-359 stupňů
			_compassDegrees = (int)(Angle.FromCartesianDegrees((double)_compassDegrees)).CompassDegrees;
			_angle = Angle.FromCompassDegrees(_compassDegrees);
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="o">The <see cref="Orientation2D"/> struct to be copied</param>
		public Orientation2D(Orientation2D o) : this(o.UnitVector) { }

		/// <summary>
		/// The orientation converted to an angle
		/// </summary>
		public Angle Angle
			=> _angle;

		/// <summary>
		/// A directional unit vector defining the orientation
		/// </summary>
		public Vector2 UnitVector
		{
			get => _unitVector;
			set
			{
				_unitVector = value;
				_unitVector.Normalize();
			}
		}

		/// <summary>
		/// Converts a <see cref="Vector3"/> struct to <see cref="Orientation2D"/>.
		/// </summary>
		/// <param name="v">The vector to convert</param>
		public static implicit operator Orientation2D(Vector3 v)
			=> new(new(v.x, v.z));

		/// <summary>
		/// Converts a <see cref="Vector2"/> struct to <see cref="Orientation2D"/>.
		/// </summary>
		/// <param name="v">The vector to convert</param>
		public static implicit operator Orientation2D(Vector2 v)
			=> new(v);

		/// <summary>
		/// Converts a <see cref="Angle"/> struct to <see cref="Orientation2D"/>.
		/// </summary>
		/// <param name="a">The angle struct to be converted</param>
		public static implicit operator Orientation2D(Angle a)
			=> new(RadiansToVector2(a));

		/// <summary>
		/// Converts a <see cref="Orientation2D"/> struct to <see cref="Vector3"/>.
		/// </summary>
		/// <param name="o">The orientation to convert</param>
		public static implicit operator Vector3(Orientation2D o)
			=> new(o.UnitVector.x, 0, o.UnitVector.y);

		/// <summary>
		/// Checks equality of two instances
		/// </summary>
		/// <param name="o">An object to compare</param>
		/// <returns>True if both instances are equal</returns>
		public override bool Equals(object o)
			=> o != null && o.GetType() == GetType() && ((Orientation2D)o).UnitVector == UnitVector;

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>The hash code for this instance</returns>
		public override int GetHashCode()
			=> _unitVector.GetHashCode();

		/// <summary>
		/// Rotates the orientation.
		/// </summary>
		/// <param name="turn">Specifies how much the orientation should be rotated</param>
		public void Rotate(TurnType turn)
			=> Rotate((int)turn);

		private Vector2 DegreesToVector(int degrees)
		{
			Dictionary<int, Vector2> angleToVector = new()
			{
				{ 0, new(1, 0) },
				{ 45, new(0.707f, 0.707f) },
				{ 90, new(0, 1) },
				{ 135, new(-0.707f, 0.707f) },
				{ 180, new(-1, 0) },
				{ 225, new(-0.707f, -0.707f) },
				{ 270, new(0, -1) },
				{ 315, new(0.707f, -0.707f) }
			};

			Vector2 result = default;
			if (angleToVector.TryGetValue(degrees, out result))
				return result;

			// Compute a new unit vector
			double radians = DegreesToRadians(_compassDegrees);
			result = new(
				(float)Math.Cos(radians),
				(float)Math.Sin(radians));

			return result;
		}

		public bool Chipotle;

		/// <summary>
		/// Rotates the orientation.
		/// </summary>
		/// <param name="degrees">Specifies how much the orientation should be rotated</param>
		/// <returns>The rotated orientation</returns>
		public Orientation2D Rotate(int degrees)
		{
			_compassDegrees = (_compassDegrees + degrees) % 360; // Adding a rotation to the current orientation
			if (_compassDegrees < 0)
				_compassDegrees += 360;
			double cartesian = CompassToCartesian((float)_compassDegrees);
			_unitVector = DegreesToVector((int)Math.Round(cartesian));
			_angle = Angle.FromCompassDegrees(_compassDegrees);

			return this;
		}

		/// <summary>
		/// Converts compas degrees to cartesian degrees.
		/// </summary>
		/// <param name="compassDegrees">The value in compass degrees</param>
		/// <returns>The angle in cartesian degrees</returns>
		private double CompassToCartesian(float compassDegrees)
			=> NormalizeDegrees(360 + 90 - compassDegrees);

		private double DegreesToRadians(double degrees)
			=> degrees * PI / 180;

		/// <summary>
		/// Converts radians to the <see cref="Vector2"/> unit vector.
		/// </summary>
		/// <param name="radians">An angle in radians to be converted</param>
		/// <returns></returns>
		private static Vector2 RadiansToVector2(double radians)
			=> new((float)Cos(radians), (float)Sin(radians));

		/// <summary>
		/// Converts a <see cref="Vector2"/> unit vector to radians.
		/// </summary>
		/// <param name="v">The unit vector to be converted</param>
		/// <returns>Result of tghe conversion</returns>
		private double VectorToRadians(Vector2 v)
			=> Atan2(v.y, v.x);
	}
}