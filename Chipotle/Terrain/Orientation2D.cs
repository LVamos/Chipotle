using Luky;

using static System.Math;

namespace Game.Terrain
{
    public struct Orientation2D
    {

        public override int GetHashCode()
            => _unitVector.GetHashCode();

        private Vector2 _unitVector;

        private float VectorToRadians(Vector2 v)
            => (float)Atan2(v.Y, v.X);

        public Angle Angle => new Angle(VectorToRadians(UnitVector));
        public Vector2 UnitVector
        {
            get => _unitVector;
            set
            {
                _unitVector = value;
                _unitVector.Normalize();
            }
        }


        private static Vector2 RadiansToVector2(double radians)
            => new Vector2((float)Cos(radians), (float)Sin(radians));



        public void Rotate(TurnType turn)
            => Rotate((int)turn);

        public void Rotate(float compassDegrees)
        {
            float radians = Angle.DegreesToRadians(-compassDegrees);
            float cos = (float)Cos(radians);
            float sin = (float)Sin(radians);

            if (compassDegrees == -90)
                _unitVector = _unitVector.PerpendicularLeft;
            else if (compassDegrees == 90)
                _unitVector = _unitVector.PerpendicularRight;
            else if (Abs(compassDegrees) == 180)
                _unitVector = _unitVector.PerpendicularRight.PerpendicularRight;
            else
                _unitVector = new Vector2(cos * _unitVector.X - sin * _unitVector.Y, sin * _unitVector.X + cos * _unitVector.Y);
        }

        public Orientation2D(float x, float y) : this(new Vector2(x, y))
        { }


        public Orientation2D(Vector2 unitVector)
        {
            _unitVector = unitVector;
            _unitVector.Normalize();
        }

        public Orientation2D(Angle angle) : this(RadiansToVector2(angle.Radians)) { }

        public Orientation2D(Orientation2D o) : this(o.UnitVector) { }


        public static implicit operator Vector3(Orientation2D o)
            => new Vector3(o.UnitVector.X, 0, o.UnitVector.Y);

        public static implicit operator Orientation2D(Vector3 v)
            => new Orientation2D(new Vector2(v.X, v.Z));

        public static implicit operator Orientation2D(Vector2 v)
            => new Orientation2D(v);

        public static implicit operator Orientation2D(Angle a)
            => new Orientation2D(RadiansToVector2(a));

        public override bool Equals(object o)
            => o != null && o.GetType() == GetType() && ((Orientation2D)o).UnitVector == UnitVector;

    }
}
