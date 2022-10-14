using OpenTK;

using ProtoBuf;

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
        /// A directional unit vecotr defining the orientation
        /// </summary>
        private Vector2 _unitVector;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">The X component of the vector defining the orientation</param>
        /// <param name="y">The Y component of the vector defining the orientation</param>
        public Orientation2D(float x, float y) : this(new Vector2(x, y))
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitVector">A directional unit vector defining the orientation</param>
        public Orientation2D(Vector2 unitVector)
        {
            _unitVector = unitVector;
            _unitVector.Normalize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="angle">An <see cref="Angle"/> struct defining the orientation</param>
        public Orientation2D(Angle angle) : this(RadiansToVector2(angle.Radians)) { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="o">The <see cref="Orientation2D"/> struct to be copied</param>
        public Orientation2D(Orientation2D o) : this(o.UnitVector) { }

        /// <summary>
        /// The orientation converted to an angle
        /// </summary>
        public Angle Angle
            => new Angle(VectorToRadians(UnitVector));

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
            => new Orientation2D(new Vector2(v.X, v.Z));

        /// <summary>
        /// Converts a <see cref="Vector2"/> struct to <see cref="Orientation2D"/>.
        /// </summary>
        /// <param name="v">The vector to convert</param>
        public static implicit operator Orientation2D(Vector2 v)
            => new Orientation2D(v);

        /// <summary>
        /// Converts a <see cref="Angle"/> struct to <see cref="Orientation2D"/>.
        /// </summary>
        /// <param name="a">The angle struct to be converted</param>
        public static implicit operator Orientation2D(Angle a)
            => new Orientation2D(RadiansToVector2(a));

        /// <summary>
        /// Converts a <see cref="Orientation2D"/> struct to <see cref="Vector3"/>.
        /// </summary>
        /// <param name="o">The orientation to convert</param>
        public static implicit operator Vector3(Orientation2D o)
            => new Vector3(o.UnitVector.X, 0, o.UnitVector.Y);

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

        /// <summary>
        /// Rotates the orientation.
        /// </summary>
        /// <param name="degrees">Specifies how much the orientation should be rotated</param>
        /// <returns>The rotated orientation</returns>
        public Orientation2D Rotate(double degrees)
        {
            if (degrees == -90)
                _unitVector = _unitVector.PerpendicularLeft;
            else if (degrees == 90)
                _unitVector = _unitVector.PerpendicularRight;
            else if (Abs(degrees) == 180)
                _unitVector = _unitVector.PerpendicularRight.PerpendicularRight;
            else
            {
                double radians = Angle.DegreesToRadians(degrees);
                double cos = Cos(radians);
                double sin = -Sin(radians);
                _unitVector = new Vector2((float)(cos * _unitVector.X - sin * _unitVector.Y), (float)(sin * _unitVector.X + cos * _unitVector.Y));
            }

            return this;
        }

        /// <summary>
        /// Converts radians to the <see cref="Vector2"/> unit vector.
        /// </summary>
        /// <param name="radians">An angle in radians to be converted</param>
        /// <returns></returns>
        private static Vector2 RadiansToVector2(double radians)
            => new Vector2((float)Cos(radians), (float)Sin(radians));

        /// <summary>
        /// Converts a <see cref="Vector2"/> unit vector to radians.
        /// </summary>
        /// <param name="v">The unit vector to be converted</param>
        /// <returns>Result of tghe conversion</returns>
        private double VectorToRadians(Vector2 v)
                                    => Atan2(v.Y, v.X);
    }
}