using Game.Entities;

using Luky;

using OpenTK;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a movable resizable rectangle region of the game map.
    /// </summary>
    /// <remarks>
    /// The region is defined by two points: <see cref="Rectangle.UpperLeftCorner"/> and <see cref="Rectangle.LowerRightCorner"/>.
    /// </remarks>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class Rectangle : DebugSO
    {
        /// <summary>
        /// Enumerates all walkable points in a distance range around this plane.
        /// </summary>
        /// <param name="minDistance"Minimum distance of the surrounding pint from the plane></param>
        /// <param name="maxDistance">Maximum distance of the surroudning points from the plane</param>
        /// <returns>Enumeration of points</returns>
        public IEnumerable<Vector2> GetWalkableSurroundingPoints(int minDistance, int maxDistance)
                => GetSurroundingPoints(minDistance, maxDistance)
                .Where(p => World.IsWalkable(p));

        /// <summary>
        /// Checks if all the plane is walkable.
        /// </summary>
        public bool Walkable()
        {
            return
                GetPoints()
                .All(p => World.IsWalkable(p));
        }

        /// <summary>
        /// Checks if the plane is a line.
        /// </summary>
        [ProtoIgnore]
        public bool IsLine
            => Size > 1 && (Width == 1 || Height == 1);

        /// <summary>
        /// Checks if the plane is a horizohntal rectangle.
        /// </summary>
        /// <returns>True if the plane is a horizontal rectangle</returns>
        [ProtoIgnore]
        public bool Horizontal
            => Width > Height;

        /// <summary>
        /// Checks if the plane is a vertical rectangle.
        /// </summary>
        /// <returns>True if the plane is a vertical rectangle</returns>
        public bool Vertical
            => Width < Height;

        /// <summary>
        /// Checks if the plane is a square.
        /// </summary>
        [ProtoIgnore]
        public bool IsSquare
            => Height == Width;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="upperLeft">Coordinates of the upper left corner of the rectangle</param>
        /// <param name="lowerRight">Coordinates of the lower right corner of the rectangle</param>
        public Rectangle(Vector2 upperLeft, Vector2 lowerRight)
        {
            UpperLeftCorner = upperLeft;
            LowerRightCorner = lowerRight;

            MinimumHeight = MinimumWidth = 1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="upperLeft">Coordinates of the upper left corner of the rectangle</param>
        /// <remarks>It creates a square of size 1.</remarks>
        public Rectangle(Vector2 upperLeft) : this(upperLeft, upperLeft)
        { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="coordinates">A string with four coordinates separated by comma</param>
        public Rectangle(string coordinates)
        {
            // Null check
            Assert(!string.IsNullOrEmpty(coordinates), $"{nameof(coordinates)} cann't be null.");

            // Parse the values
            List<int> coordinateList = new List<int>();
            coordinates = coordinates.Trim();
            foreach (string coordinate in Regex.Split(coordinates, @"\D+"))
                coordinateList.Add(int.Parse(coordinate));

            // Check format
            Assert(coordinateList.Count == 4 || coordinateList.Count == 2, $"{nameof(coordinates)} in invalid format");

            UpperLeftCorner = new Vector2(coordinateList[0], coordinateList[1]);
            LowerRightCorner = coordinateList.Count == 2 ? UpperLeftCorner : new Vector2(coordinateList[2], coordinateList[3]);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="plane">Another plane to be copied</param>
        public Rectangle(Rectangle plane) : this(plane.UpperLeftCorner, plane.LowerRightCorner)
        {
            MinimumHeight = plane.MinimumHeight;
            MinimumWidth = plane.MinimumWidth;
        }

        /// <summary>
        /// Returns coordinates of the center of the plane.
        /// </summary>
        [ProtoIgnore]
        public Vector2 Center
            => Size == 1 ? UpperLeftCorner : new Vector2((UpperLeftCorner.X + LowerRightCorner.X) / 2, (UpperLeftCorner.Y + LowerRightCorner.Y) / 2);

        /// <summary>
        /// Height of the plane.
        /// </summary>
        [ProtoIgnore]
        public float Height => 1 + UpperLeftCorner.Y - LowerRightCorner.Y;

        /// <summary>
        /// Returns coordinates of the lower left corner of the plane.
        /// </summary>
        [ProtoIgnore]
        public Vector2 LowerLeftCorner
            => new Vector2(UpperLeftCorner.X, LowerRightCorner.Y);

        /// <summary>
        /// Gets or sets the lower right corner of the plane.
        /// </summary>
        public Vector2 LowerRightCorner { get; set; }

        /// <summary>
        /// Specifies the minimum permitted height of the plane.
        /// </summary>
        public float MinimumHeight { get; set; }

        /// <summary>
        /// Specifies the minimum permitted width of the plane.
        /// </summary>
        public float MinimumWidth { get; set; }

        /// <summary>
        /// Returns size of the plane.
        /// </summary>
        [ProtoIgnore]
        public float Size
            => Height * Width;

        /// <summary>
        /// Coordinates of the upper left corner of the plane.
        /// </summary>
        public Vector2 UpperLeftCorner { get; set; }

        /// <summary>
        /// Coordinates of the upper right corner of the plane.
        /// </summary>
        [ProtoIgnore]
        public Vector2 UpperRightCorner => new Vector2(LowerRightCorner.X, UpperLeftCorner.Y);

        /// <summary>
        /// Returns width of the plane.
        /// </summary>
        public float Width
            => 1 + LowerRightCorner.X - UpperLeftCorner.X;

        /// <summary>
        /// Converts relative coordinates to absolute coordinates.
        /// </summary>
        /// <param name="relative">The coordinates related to upper left corner of <paramref name="area"/></param>
        /// <param name="area">
        /// The locality according to which <paramref name="relative"/> was calculated
        /// </param>
        /// <returns>New isntance of <see cref="Rectangle"/> defined by the absolute coordinates</returns>
        public static Vector2 GetAbsoluteCoordinates(Vector2 relative, Rectangle area)
                    => new Vector2(area.UpperLeftCorner.X + relative.X, area.UpperLeftCorner.Y - relative.Y);

        /// <summary>
        /// Converts absolute coordinates to relative coordinates.
        /// </summary>
        /// <param name="absolute">Absolute coordiantes to be converted</param>
        /// <returns></returns>
        public static Vector2 GetRelativeCoordinates(Vector2 absolute)
        {
            Rectangle locality = World.GetLocality(absolute).Area;
            locality.ArrangeCorners();
            Vector2 corner = locality.UpperLeftCorner;
            return new Vector2(absolute.X - corner.X, corner.Y - absolute.Y);
        }

        /// <summary>
        /// Verifies that <see cref="UpperLeftCorner"/> points more to the left and more upper than
        /// the <see cref="LowerRightCorner"/> and rearranges them if necessary.
        /// </summary>
		public void ArrangeCorners()
        {
            float temp;
            if (UpperLeftCorner.X > LowerRightCorner.X)
            { // Swap them
                temp = UpperLeftCorner.X;
                UpperLeftCorner = new Vector2(LowerRightCorner.X, UpperLeftCorner.Y);
                LowerRightCorner = new Vector2(temp, LowerRightCorner.Y);
            }

            if (UpperLeftCorner.Y < LowerRightCorner.Y)
            { // Swap them
                temp = UpperLeftCorner.Y;
                UpperLeftCorner = new Vector2(UpperLeftCorner.X, LowerRightCorner.Y);
                LowerRightCorner = new Vector2(LowerRightCorner.X, temp);
            }
        }

        /// <summary>
        /// Checks if the given plane fully intersects this plane.
        /// </summary>
        /// <param name="area">The plane to be checked</param>
        /// <returns>True if the given plane fully intersects this plane</returns>
        public bool Contains(Rectangle area)
            => Intersects(area.UpperLeftCorner) && Intersects(area.LowerRightCorner);

        /// <summary>
        /// Checks if both instances are equal.
        /// </summary>
        /// <param name="p">The other plane to compare</param>
        /// <returns>True if both instances are equal</returns>
        public bool Equals(Rectangle p)
            => UpperLeftCorner == p.UpperLeftCorner && LowerRightCorner == p.LowerRightCorner;

        /// <summary>
        /// Extends the plane to all directions by the specified amount of units in all directions.
        /// </summary>
        /// <param name="units">Amount of the units by which the rectangle is extended</param>
        public void Extend(int units = 1)
        {
            for (int i = 1; i <= units; i++)
            {
                UpperLeftCorner += Direction.UpLeft.AsVector2();
                LowerRightCorner += Direction.DownRight.AsVector2();
            }
        }

        /// <summary>
        /// Extends the plane in the specified direction by one unit.
        /// </summary>
        /// <param name="direction">Side of the plane to be moved</param>
        public void Extend(Direction direction)
        {
            Vector2 step = direction.AsVector2();
            if (direction == Direction.Left || direction == Direction.Up)
                UpperLeftCorner += step;
            else if (direction == Direction.Right || direction == Direction.Down)
                LowerRightCorner += step;
        }

        /// <summary>
        /// Returns the point of this plane closest to the default point.
        /// </summary>
        /// <param name="point">Coordinates of the point whose surroundings are to be explored</param>
        /// <returns>The point closest to the default point toward this plane</returns>
        public Vector2 GetClosestPoint(Vector2 point)
            => GetPointsByDistance(point).First();

        /// <summary>
        /// Returns distance between the plane and the default point.
        /// </summary>
        /// <param name="point">The point from which the distance is to be calculated</param>
        /// <returns>The distance between the default point and the plane</returns>
        public float GetDistanceFrom(Vector2 point)
            => World.GetDistance(point, GetClosestPoint(point));

        /// <summary>
        /// Returns hash code of this instance.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
                            => unchecked(4112 * (8121 + UpperLeftCorner.GetHashCode()) * (6988 + LowerRightCorner.GetHashCode()));

        /// <summary>
        /// Enumerates entities intersecting with the plane.
        /// </summary>
        /// <returns>Enumeration of intersecting entities</returns>
        public IEnumerable<Character> GetEntities()
                    => World.GetEntities(this);

        /// <summary>
        /// Returns all game objects intersecting with the plane.
        /// </summary>
        /// <returns>List of intersecting objects</returns>
        public IEnumerable<GameObject> GetObjects()
            => World.GetObjects(this);

        /// <summary>
        /// Returns all passages intersecting with the plane.
        /// </summary>
        /// <returns>List of intersecting passages</returns>
        public IEnumerable<Passage> GetPassages()
            => World.GetPassages(this);

        /// <summary>
        /// Identifies a perimeter side of the plane on which the specified point lays.
        /// </summary>
        /// <param name="point">The point to be checked</param>
        /// <returns>
        /// A <see cref="Direction"/> enum identifying the side on which the specified point lays.
        /// </returns>
        public Direction GetIntersectingSide(Vector2 point)
            => DirectionExtension.BasicDirections
            .Where(d => IntersectsWithSide(d, point))
            .FirstOrDefault();

        /// <summary>
        /// Enumerates all localities this plane intersects with.
        /// </summary>
        /// <returns>enumeration of the intersecting localities</returns>
        public IEnumerable<Locality> GetLocalities() => World.GetLocalities(this);

        /// <summary>
        /// Enumerates all tiles laying on the perimeter of this plane.
        /// </summary>
        public IEnumerable<Tile> GetPerimeter()
        {
            foreach (Vector2 c in GetPerimeterPoints())
                yield return World.Map[c];
        }

        /// <summary>
        /// Enumerates all point laying on the perimeter of this plane.
        /// </summary>
        /// <returns>all point laying on the perimeter of this plane</returns>
        public IEnumerable<Vector2> GetPerimeterPoints()
=> GetPerimeterPoints(Direction.Left).Union(GetPerimeterPoints(Direction.Up)).Union(GetPerimeterPoints(Direction.Right)).Union(GetPerimeterPoints(Direction.Down));

        /// <summary>
        /// Enumerates the perimeter points lying on the specified sides of this plane.
        /// </summary>
        /// <param name="sides">Perimeter sides whose points are to be returned</param>
        /// <returns>the perimeter points lying on the specified sides of this plane</returns>
        public IEnumerable<Vector2> GetPerimeterPoints(IEnumerable<Direction> sides)
            => sides.Select(d => GetPerimeterPoints(d)).SelectMany(p => p).Distinct();

        /// <summary>
        /// Enumerates the perimeter points laying on the specified side.
        /// </summary>
        /// <param name="side">The perimeter side whose points are to be enumerated</param>
        /// <returns>the perimeter points laying on the specified side</returns>
        public IEnumerable<Vector2> GetPerimeterPoints(Direction side)
        {
            float x, y;
            float left = UpperLeftCorner.X;
            float right = LowerRightCorner.X;
            float top = UpperLeftCorner.Y;
            float bottom = LowerRightCorner.Y;

            switch (side)
            {
                case Direction.Left:
                    for (y = bottom; y <= top; y++)
                        yield return new Vector2(left, y);

                    break;

                case Direction.Right:
                    for (y = bottom; y <= top; y++)
                        yield return new Vector2(right, y);

                    break;

                case Direction.Up:
                    for (x = left; x <= right; x++)
                        yield return new Vector2(x, top);

                    break;

                case Direction.Down:
                    for (x = left; x <= right; x++)
                        yield return new Vector2(x, bottom);

                    break;

                default:
                    throw new ArgumentException(nameof(side));
            }
        }

        /// <summary>
        /// Enumerates all tiles laying on the perimeter of this plane.
        /// </summary>
        /// <returns>all tiles laying on the perimeter of this plane</returns>
        public IEnumerable<(Vector2 position, Tile tile)> GetPerimeterTiles()
        {
            IEnumerable<(Vector2 position, Tile tile)> points = GetPerimeterPoints()
                .Select(p => (p, World.Map[p]));
            return points.Where(t => t.tile != null);
        }

        /// <summary>
        /// Enumerates all tiles laying on the specified side of this plane.
        /// </summary>
        /// <param name="side">Side of the plane to be enumerated</param>
        /// <returns>all tiles laying on the specified side of this plane</returns>
        public IEnumerable<Tile> GetPerimeterTiles(Direction side)
=> GetPerimeterPoints(side)
            .Select(p => World.Map[p])
            .Where(t => t != null);

        /// <summary>
        /// Enumerates all points of the plane.
        /// </summary>
        /// <returns>all points of the plane</returns>
        public IEnumerable<Vector2> GetPoints()
        {
            Vector2 position;
            for (position.X = UpperLeftCorner.X; position.X <= LowerRightCorner.X; position.X++)
            {
                for (position.Y = LowerRightCorner.Y; position.Y <= UpperLeftCorner.Y; position.Y++)
                    yield return position;
            }
        }

        /// <summary>
        /// Enumerates all points sorted by distance from the default point.
        /// </summary>
        /// <param name="point">Coordinates of the default point whose surroundings should be explored.</param>
        /// <returns>Coordinates</returns>
        public IEnumerable<Vector2> GetPointsByDistance(Vector2 point)
            => GetPoints().OrderBy(p => World.GetDistance(p, point));

        /// <summary>
        /// Returns a new plane corresponding to coordinates of the specified perimeter side of this plane.
        /// </summary>
        /// <param name="side">The side of this plane to be copied</param>
        /// <returns>A new plane corresponding to the specified perimeter side of this plane</returns>
        public Rectangle GetSide(Direction side)
        {
            switch (side)
            {
                case Direction.Left: return new Rectangle(UpperLeftCorner, LowerLeftCorner);
                case Direction.Up: return new Rectangle(UpperLeftCorner, UpperRightCorner);
                case Direction.Right: return new Rectangle(UpperRightCorner, LowerRightCorner);
                case Direction.Down: return new Rectangle(LowerLeftCorner, LowerRightCorner);
                default: return null;
            }
        }

        /// <summary>
        /// Returns two points that define the specified perimeter side of this plane.
        /// </summary>
        /// <param name="side">The side to be copied</param>
        /// <returns>A tuple with two points defining the specified perimeter side of this plane</returns>
        public (Vector2 Point1, Vector2 Point2) GetSideDefinition(Direction side)
        {
            switch (side)
            {
                case Direction.Left: return (LowerLeftCorner, UpperLeftCorner);
                case Direction.Up: return (UpperLeftCorner, UpperRightCorner);
                case Direction.Right: return (LowerRightCorner, UpperRightCorner);
                case Direction.Down: return (LowerLeftCorner, LowerRightCorner);
                default: throw new ArgumentException(nameof(side));
            }
        }

        /// <summary>
        /// Enumerates all closest points from surroundings of this plane.
        /// </summary>
        /// <param name="minDistance">Specifies minimum distance of the surrounding points from edges of the specified plane.</param>
        /// <param name="maxDistance">Specifies maximum distance of the surrounding points from edges of the specified plane.</param>
        /// <returns>all closest points from surroundings of this plane</returns>
        public IEnumerable<Vector2> GetSurroundingPoints(int minDistance, int maxDistance)
        {
            Rectangle surroundings = new Rectangle(this);
            surroundings.Extend(maxDistance);

            return
                (from p in surroundings.GetPoints()
                 let distance = World.GetDistance(GetClosestPoint(p), p)
                 where !Intersects(p) && distance >= minDistance && distance <= maxDistance
                 select p);

            return null;
        }

        /// <summary>
        /// Checks if the specified point is opposite to one of sides of the plane.
        /// </summary>
        /// <param name="point">The point to be checked</param>
        /// <returns><True if the point is opposite to one of the sides of the plane/returns>
        public bool IsOpposite(Vector2 point)
        {
            return
            !Intersects(point)
            && ((point.X >= UpperLeftCorner.X && point.X <= UpperRightCorner.X)
            || (point.Y >= LowerLeftCorner.Y && point.Y <= UpperLeftCorner.Y));
        }

        /// <summary>
        /// Checks if the specified plane is opposite to this plane.
        /// </summary>
        /// <param name="p">The plane to be checked</param>
        /// <returns>True if the specified plane is opposite to this plane</returns>
        public bool IsOpposite(Rectangle p)
            => IsOpposite(p.UpperLeftCorner) || IsOpposite(p.LowerRightCorner);

        /// <summary>
        /// Selects a point from the specified plane which is opposite to this plane.
        /// </summary>
        /// <param name="plane">The plane to be checked</param>
        /// <returns>The opposite point or null</returns>
        public Vector2? FindOppositePoint(Rectangle plane)
        {
            foreach (Vector2 point in plane.GetPoints())
            {
                if (IsOpposite(point))
                    return (Vector2)point;
            }

            return null;
        }

        /// <summary>
        /// Enumerates all closest tiles from surroundings of this plane.
        /// </summary>
        /// <param name="minDistance">Minimum distance of the surrounding points from the plane</param>
        /// <param name="maxDistance">Maximum distance of the surrounding tiles from the plane</param>
        /// <returns>all closest tiles from surroundings of this plane</returns>
        public IEnumerable<Tile> GetSurroundingTiles(int minDistance, int maxDistance)
            => GetSurroundingPoints(minDistance, maxDistance).Select(p => World.Map[p]).Where(t => t != null);

        /// <summary>
        /// Enumerates all tiles intersecting with the plane.
        /// </summary>
        /// <returns>all tiles intersecting with the plane</returns>
        public IEnumerable<(Vector2 position, Tile tile)> GetTiles()
        {
            IEnumerable<(Vector2 position, Tile tile)> tiles = GetPoints().Select(p => (p, World.Map[p]));
            return tiles.Where(p => p.tile != null);
        }

        /// <summary>
        /// Returns a directional vector parallel with the specified side.
        /// </summary>
        /// <param name="side">The side whose directional vector is to be returned</param>
        /// <returns>a directional vector parallel with the specified side</returns>
        public Vector2 GetVectorOfSide(Direction side)
        {
            (Vector2 Point1, Vector2 Point2) = GetSideDefinition(side);
            return Point2 - Point1;
        }

        /// <summary>
        /// Enumerates all walkable intersecting tiles.
        /// </summary>
        /// <returns>all walkable intersecting tiles</returns>
        public IEnumerable<(Vector2 position, Tile tile)> GetWalkableTiles()
            => GetTiles().Where(t => World.IsWalkable(t.position));

        /// <summary>
        /// Checks if this plane intersects with the specified plane.
        /// </summary>
        /// <param name="plane">The plane to be compared</param>
        /// <returns>True if the planes intersect with each other</returns>
        public bool Intersects(Rectangle plane)
        {
            foreach (Vector2 c in GetPoints())
            {
                if (plane.Intersects(c))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the specified point lays on the specified perimeter side of this plane.
        /// </summary>
        /// <param name="side">The perimeter side to be checked</param>
        /// <param name="point">The point to be checked</param>
        /// <returns>if the specified point lays on the specified perimeter side of this plane</returns>
        public bool IntersectsWithSide(Direction side, Vector2 point)
        {
            (Vector2 Point1, Vector2 Point2) = GetSideDefinition(side);
            return point.X >= Point1.X && point.Y >= Point1.Y && point.X <= Point2.X && point.Y <= Point2.Y;
        }

        /// <summary>
        /// Checks if the specified point lays on the periemter of this plane.
        /// </summary>
        /// <param name="point">The point to be checked</param>
        /// <returns>if the specified point lays on the periemter of this plane</returns>
        public bool IsOnPerimeter(Vector2 point)
            => DirectionExtension.BasicDirections
            .Any(d => IntersectsWithSide(d, point));

        /// <summary>
        /// Checks if the specified point lays on this plane.
        /// </summary>
        /// <param name="point">The point to be checked</param>
        /// <returns>True if the point lays on the plane</returns>
        public bool Intersects(Vector2 point)
            => point.X >= UpperLeftCorner.X && point.X <= LowerRightCorner.X && point.Y >= LowerRightCorner.Y && point.Y <= UpperLeftCorner.Y;

        /// <summary>
        /// Transforms plane coordinates by one unit to the specified direction.
        /// </summary>
        /// <param name="direction">The direction where the plane is to be moved</param>
        public Rectangle Move(Direction direction)
            => Move(direction.AsVector2());

        /// <summary>
        /// Transforms plane coordinates to the specified direction.
        /// </summary>
        /// <param name="direction">
        /// The direction defined by a unit vector where the plane is to be moved
        /// </param>
        /// <param name="step">Specifies length of the transformation</param>
        public Rectangle Move(Vector2 direction, float step)
        {
            direction *= step;
            UpperLeftCorner += direction;
            LowerRightCorner += direction;
            return this;
        }

        /// <summary>
        /// Transforms plane coordinates by one unit to the specified direction.
        /// </summary>
        /// <param name="direction">
        /// The direction defined by an unitvector where the plane is to be moved
        /// </param>
        public Rectangle Move(Vector2 direction)
            => Move(direction, 1f);

        /// <summary>
        /// Transforms plane coordinates to the specified direction.
        /// </summary>
        /// <param name="direction">
        /// The direction defined by an <see cref="Orientation2D"/> struct where the plane is to be moved
        /// </param>
        /// <param name="step">Specifies length of the transformation</param>
        public void Move(Orientation2D direction, float step)
            => Move(direction.UnitVector, step);

        /// <summary>
        /// Transforms plane coordinates one unit to the specified direction.
        /// </summary>
        /// <param name="direction">
        /// The direction defined by an <see cref="Orientation2D"/> struct where the plane is to be moved
        /// </param>
        public void Move(Orientation2D direction)
            => Move(direction, 1f);

        /// <summary>
        /// Reduces the plane in given direction. Moves one side.
        /// </summary>
        /// <param name="direction">Direction of the reduction</param>
        public void Reduce(Direction direction)
        {
            Assert((direction.IsVertical() && Height >= MinimumHeight) || (direction.IsHorizontal() && Width >= MinimumWidth), "Minimum size exceeded.");

            if (((direction == Direction.Left || direction == Direction.Right) && Width == 1) || ((direction == Direction.Up || direction == Direction.Down) && Height == 1))
                return;

            Vector2 step = direction.GetOpposite().AsVector2();
            if (direction == Direction.Left || direction == Direction.Up)
                UpperLeftCorner += step;
            else if (direction == Direction.Right || direction == Direction.Down)
                LowerRightCorner += step;
        }

        /// <summary>
        /// Converts the plane to a plane with absolute coordinations.
        /// </summary>
        /// <param name="area">The plane according to which coordinates of this plane were calculated</param>
        /// <returns>A new plane with absolute coordinates</returns>
        /// <remarks>Considers this plane to be relative</remarks>
        public Rectangle ToAbsolute(Rectangle area)
            => new Rectangle(GetAbsoluteCoordinates(UpperLeftCorner, area), GetAbsoluteCoordinates(LowerRightCorner, area));

        /// <summary>
        /// converts this plane to a plane with relative coordinates.
        /// </summary>
        /// <returns>A new plane with relative coordinates</returns>
        /// <remarks>Considers this plane to be absolute</remarks>
        public Rectangle ToRelative()
                                                                                                                        => new Rectangle(GetRelativeCoordinates(UpperLeftCorner), GetRelativeCoordinates(LowerRightCorner));

        /// <summary>
        /// Returns coordinates of the plane as a string.
        /// </summary>
        /// <returns>coordinates of the plane as a comma separated string</returns>
        public override string ToString()
=> $"{UpperLeftCorner}, {LowerRightCorner}";
    }
}