using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Game.Entities;

using Luky;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a rectangle slice of tile map from static class named World. Should be used for
    /// exploring or searching the map. It's guaranted that top left corner has always lower
    /// coordinates than bottom right one.
    /// </summary>
    public class Plane : DebugSO
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="topLeft">Coordinates of top left corner of the rectangle</param>
        /// <param name="bottomRight">Coordinates of bottom right corner of the rectangle</param>
        public Plane(Vector2 topLeft, Vector2 bottomRight)
        {
            UpperLeftCorner = topLeft;
            LowerRightCorner = bottomRight;

            MinimumHeight = MinimumWidth = 1;
        }

        public Plane(Vector2 coordinates) : this(coordinates, coordinates)
        { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="coordinates">
        /// Coordinates of top left and bottom right corners of new rectangle divided by comas or
        /// spaces. Should contaion four numbers: first two for top left and two others for bottom
        /// right corner.
        /// </param>
        public Plane(string coordinates)
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

        public Plane(Plane plane) : this(plane.UpperLeftCorner, plane.LowerRightCorner)
        {
            MinimumHeight = plane.MinimumHeight;
            MinimumWidth = plane.MinimumWidth;
        }

        public Vector2 Center => Size == 1 ? UpperLeftCorner : new Vector2((UpperLeftCorner.X + LowerRightCorner.X) / 2, (UpperLeftCorner.Y + LowerRightCorner.Y) / 2);

        public float Height => 1 + UpperLeftCorner.Y - LowerRightCorner.Y;

        public Vector2 LowerLeftCorner => new Vector2(UpperLeftCorner.X, LowerRightCorner.Y);

        /// <summary>
        /// Defines the rectangle
        /// </summary>
        public Vector2 LowerRightCorner { get; set; }

        public float MinimumHeight { get; set; }

        public float MinimumWidth { get; set; }

        public float Size => Height * Width;

        /// <summary>
        /// Defines the rectangle
        /// </summary>
        public Vector2 UpperLeftCorner { get; set; }

        //=> !(from c in GetCoordinates() where (plane.LaysOnPlane(c)) select c).IsNullOrEmpty();
        public Vector2 UpperRightCorner => new Vector2(LowerRightCorner.X, UpperLeftCorner.Y);

        public float Width => 1 + LowerRightCorner.X - UpperLeftCorner.X;

        public Tile this[int x, int y] { get => World.Map[x, y]; set => World.Map[x, y] = value; }

        public static Vector2 GetAbsoluteCoordinates(Vector2 relative, Locality locality)
                    => new Vector2(locality.Area.UpperLeftCorner.X + relative.X, locality.Area.UpperLeftCorner.Y - relative.Y);

        public static Vector2 GetRelativeCoordinates(Vector2 absolute)
        {
            World.Map[absolute].Locality.Area.ArrangeCorners();
            Vector2 corner = World.Map[absolute].Locality.Area.UpperLeftCorner;
            return new Vector2(absolute.X - corner.X, corner.Y - absolute.Y);
        }

        /// <summary>
        /// Transforms relative coordinates of one plane to absolute coordinates according to second plane.
        /// </summary>
        /// <param name="a">Plane with relative coordinates</param>
        /// <param name="b">Default plane</param>
        /// <returns>New plane with absolute coordinates</returns>
        public static Plane ToAbsolute(Plane a, Plane b)
        {
            Vector2 absoluteTopLeft = b.UpperLeftCorner;
            Vector2 absoluteBottomRight = b.UpperLeftCorner;

            absoluteTopLeft.X += a.UpperLeftCorner.X;
            absoluteTopLeft.Y -= a.UpperLeftCorner.Y;
            absoluteBottomRight.X += a.LowerRightCorner.X;
            absoluteBottomRight.Y -= a.LowerRightCorner.Y;

            Assert(b.LaysOnPlane(absoluteTopLeft) && b.LaysOnPlane(absoluteBottomRight), $"Plane {nameof(a)} is larger then plane {nameof(b)}.");

            return new Plane(absoluteTopLeft, absoluteBottomRight);
        }

        /// <summary>
        /// Verify that UpperLeft is more on the left and upper than LowerRight
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
        /// Associates tiles with a locality
        /// </summary>
        /// <param name="l">Containing locality</param>
        public void Associate(Locality l)
=> GetTiles().Foreach(t => t.Register(l));

        public bool ContainsEmptyTiles()
        => GetPoints().Any(p => World.Map[p] == null);

        /// <summary>
        /// Extends the plane to all directions by one unit.
        /// </summary>
        public void Extend()
        {
            UpperLeftCorner += Direction.UpLeft.AsVector2();
            LowerRightCorner += Direction.DownRight.AsVector2();
        }

        /// <summary>
        /// Extends the plane in specified direction by one unit.
        /// </summary>
        /// <param name="direction">To which side should it be extended?</param>
        public void Extend(Direction direction)
        {
            Vector2 step = direction.AsVector2();
            if (direction == Direction.Left || direction == Direction.Up)
                UpperLeftCorner += step;
            else if (direction == Direction.Right || direction == Direction.Down)
                LowerRightCorner += step;
        }

        /// <summary>
        /// Finds nearest walkable tile in surroundings.
        /// </summary>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public Vector2? FindNearestWalkableTile(int maxDistance)
        {
            Plane around = new Plane(this);
            for (int i = 0; i <= maxDistance; i++)
            {
                around.Extend();
                Tile walkable = around.GetPerimeterTiles().FirstOrDefault(t => t.Walkable);

                if (walkable != null)
                    return (Vector2?)walkable.Position;
            }
            return (Vector2?)null;
        }

        /// <summary>
        /// Searches nearest surrounding of plane and select random walkable tile.
        /// </summary>
        /// <param name="maxDistance">Specifies maximum distance from plane</param>
        /// <returns>Coordinates of a walkable tile or null if no tile is found</returns>
        public Vector2? FindRandomWalkableTile(int maxDistance)
        {
            Plane around = new Plane(
                new Vector2(UpperLeftCorner.X - maxDistance, UpperLeftCorner.Y + maxDistance),
                new Vector2(LowerRightCorner.X + maxDistance, LowerRightCorner.Y - maxDistance));
            IEnumerable<Tile> walkables = around.GetWalkableTiles();

            if (walkables.IsNullOrEmpty())
                return (Vector2?)null;

            int index = (new Random()).Next(walkables.Count());
            return (Vector2?)walkables.ElementAt(index).Position;
        }

        /// <summary>
        /// Returns coordinates of this area which are the closest to default point.
        /// </summary>
        /// <param name="defaultPoint">Coordinates of the point whose surroundings are to be explored</param>
        /// <returns>Closest coordinates</returns>
        public Vector2 GetClosestPointTo(Vector2 defaultPoint)
            => GetPointsByDistance(defaultPoint).First();

        /// <summary>
        /// Returns distance between area and default point.
        /// </summary>
        /// <param name="defaultPoint">Coordinates of the default point</param>
        /// <returns>Distance in meters</returns>
        public float GetDistanceFrom(Vector2 defaultPoint)
            => (defaultPoint - GetClosestPointTo(defaultPoint)).LengthSquared;

        public override int GetHashCode()
                            => unchecked(4112 * (8121 + UpperLeftCorner.GetHashCode()) * (6988 + LowerRightCorner.GetHashCode()));

        public IEnumerable<Locality> GetIntersectingLocalities()
            => GetTiles().Select(t => t.Locality).Distinct();

        public IEnumerable<GameObject> GetIntersectingObjects()
            => (from t in GetTiles() where (t != null && t.Object != null) select t.Object).Distinct();

        public IEnumerable<Passage> GetIntersectingPassages()
            => (from t in GetTiles() where (t?.Passage != null) select t.Passage).Distinct();

        public Direction GetIntersectingSide(Vector2 point)
            => DirectionExtension.BasicDirections.Where(d => IntersectsWithSide(d, point)).FirstOrDefault();

        public Locality GetLocality()
            => IsInOneLocality() ? GetTiles().First().Locality : null;

        /// <summary>
        /// All tiles laying on perimeter of the locality rectangle.
        /// </summary>
        public IEnumerable<Tile> GetPerimeter()
        {
            foreach (Vector2 c in GetPerimeterPoints())
                yield return World.Map[c];
        }

        public Vector2 GetPerimeterPoint(Vector2 start, Direction direction)
=> GetPerimeterPoints(direction).Where(p => p.X == start.X || p.Y == start.Y).First();

        public IEnumerable<Vector2> GetPerimeterPoints()
=> GetPerimeterPoints(Direction.Left).Union(GetPerimeterPoints(Direction.Up)).Union(GetPerimeterPoints(Direction.Right)).Union(GetPerimeterPoints(Direction.Down));

        public IEnumerable<Vector2> GetPerimeterPoints(IEnumerable<Direction> directions)
            => directions.Select(d => GetPerimeterPoints(d)).SelectMany(p => p).Distinct();

        public IEnumerable<Vector2> GetPerimeterPoints(Direction direction)
        {
            float x, y;
            float left = UpperLeftCorner.X;
            float right = LowerRightCorner.X;
            float top = UpperLeftCorner.Y;
            float bottom = LowerRightCorner.Y;

            switch (direction)
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
                    throw new ArgumentException(nameof(direction)); break;
            }
        }

        public IEnumerable<Tile> GetPerimeterTiles()
            => GetPerimeterPoints().Select(p => World.Map[p]).Where(t => t != null);

        /// <summary>
        /// Enumerates all tiles of specified side of the plane.
        /// </summary>
        /// <param name="side">Side of the plane to be enumerated</param>
        /// <returns>Enumeration of tiles</returns>
        public IEnumerable<Tile> GetPerimeterTiles(Direction side)
=> GetPerimeterPoints(side).Select(p => World.Map[p]).Where(t => t != null);

        /// <summary>
        /// Enumerates coordinates of all tiles in the area.
        /// </summary>
        /// <returns>Enumeration of vectors</returns>
        public IEnumerable<Vector2> GetPoints()
        {
            Vector2 position;
            for (position.X = UpperLeftCorner.X; position.X <= LowerRightCorner.X; position.X++)
            {
                for (position.Y = LowerRightCorner.Y; position.Y <= UpperLeftCorner.Y; position.Y++)
                {
                    yield return position;
                }
            }
        }

        /// <summary>
        /// Returns coordinates closest to given point.
        /// </summary>
        /// <param name="defaultPoint">
        /// Coordinates of the default point whose surroundings should be explored.
        /// </param>
        /// <returns>Coordinates</returns>
        public IEnumerable<Vector2> GetPointsByDistance(Vector2 defaultPoint)
            => GetPoints().OrderBy(c => (defaultPoint - c).LengthSquared);

        /// <summary>
        /// Enumerates all coordinates in given direction which belong to this plane.
        /// </summary>
        /// <param name="start">Start coordinates</param>
        /// <param name="side">Direction of the range</param>
        /// <returns>Enumeration of coordinates</returns>
        public IEnumerable<Vector2> GetPointsToEdge(Vector2 start, Direction side)
            => World.Map.GetPointsToEdge(start, side).Where(p => LaysOnPlane(p));

        /// <summary>
        /// Creates new plane from specified side of this plane.
        /// </summary>
        /// <param name="side">Side of this plane</param>
        /// <returns>a new plane</returns>
        public Plane GetSide(Direction side)
        {
            switch (side)
            {
                case Direction.Left: return new Plane(UpperLeftCorner, LowerLeftCorner);
                case Direction.Up: return new Plane(UpperLeftCorner, UpperRightCorner);
                case Direction.Right: return new Plane(UpperRightCorner, LowerRightCorner);
                case Direction.Down: return new Plane(LowerLeftCorner, LowerRightCorner);
                default: return null;
            }
        }

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

        public IEnumerable<Vector2> GetSurroundingPoints()
        {
            Vector2 largerUpperLeft = UpperLeftCorner + Direction.UpLeft.AsVector2();
            Vector2 largerLowerRight = LowerRightCorner + Direction.DownRight.AsVector2();

            Vector2 point;
            for (point.X = largerUpperLeft.X; point.X <= largerLowerRight.X; point.X++)
            {
                for (point.Y = largerLowerRight.Y; point.Y < largerUpperLeft.Y; point.Y++)
                    yield return point;
            }
        }

        public IEnumerable<Tile> GetSurroundingTiles()
            => GetSurroundingPoints().Select(p => World.Map[p]).Where(t => t != null);

        /// <summary>
        /// Returns all tiles included in rectangle
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tile> GetTiles()
=> GetPoints().Select(p => World.Map[p]).Where(p => p != null);

        public Vector2 GetVectorOfSide(Direction side)
        {
            (Vector2 Point1, Vector2 Point2) sideDefinition = GetSideDefinition(side);
            return sideDefinition.Point2 - sideDefinition.Point1;
        }

        public IEnumerable<Tile> GetWalkableTiles()
            => GetTiles().Where(t => t.Walkable);

        /// <summary>
        /// Checks if this plane intersects with another plane.
        /// </summary>
        /// <param name="panel">The plane to be compared</param>
        /// <returns>True if the planes intersect with each other</returns>
        public bool Intersects(Plane plane)
        {
            foreach (Vector2 c in GetPoints())
            {
                if (plane.LaysOnPlane(c))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IntersectsWithAnObject()
=> !GetIntersectingObjects().IsNullOrEmpty();

        public bool IntersectsWithPassages()
            => !(from t in GetTiles() where (t != null && t.Passage != null) select t).IsNullOrEmpty();

        public bool IntersectsWithSide(Direction side, Vector2 point)
        {
            (Vector2 Point1, Vector2 Point2) definition = GetSideDefinition(side);
            return point.X >= definition.Point1.X && point.Y >= definition.Point1.Y && point.X <= definition.Point2.X && point.Y <= definition.Point2.Y;
        }

        public bool IsEmpty()
=> !GetTiles().Any(t => t != null);

        public bool IsInMapBoundaries()
                                                                                                                                                                                                                                    => World.Map.IsInBoundaries(UpperLeftCorner) && World.Map.IsInBoundaries(LowerRightCorner);

        public bool IsInOneLocality()
            => GetTiles().All(t => t != null && t.Locality == GetTiles().First().Locality);

        public bool IsNegative()
=> GetPoints().Any(c => c.IsNegative());

        /// <summary>
        /// Checks if the plane fully or partially lays on map edge.
        /// </summary>
        /// <returns>True if the plane touches edge of the map</returns>
        public bool IsOnMapEdge()
            => World.Map.IsOnEdge(UpperLeftCorner) || World.Map.IsOnEdge(LowerRightCorner);

        public bool IsOnPerimeter(Vector2 point)
            => DirectionExtension.BasicDirections.Any(d => IntersectsWithSide(d, point));

        public bool IsOnPerimeter(Plane area)
        {
            foreach (Vector2 c in area.GetPoints())
            {
                if (!IsOnPerimeter(c))
                    return false;
            }

            return true;
        }

        public bool IsPerpendicularToSide(Direction side, Vector2 vector)
        {
            Vector2 sideLineVector = GetVectorOfSide(side);
            sideLineVector.Normalize();

            return vector == sideLineVector.PerpendicularLeft || vector == sideLineVector.PerpendicularLeft;
        }

        //return new Plane(new Vector2(corner.X + UpperLeftCorner.X, corner.Y - UpperLeftCorner.Y), new Vector2(corner.X + LowerRightCorner.X, corner.Y - LowerRightCorner.Y));
        /// <summary>
        /// Checks if specified plane fully intersects with thiis plane.
        /// </summary>
        /// <param name="plane">The plane to compare</param>
        /// <returns>True if the plane lays on this plane</returns>
        public bool LaysOnPlane(Plane plane) => (from c in GetPoints()
                                                 where (!LaysOnPlane(c))
                                                 select c).Take(1).IsNullOrEmpty();

        /// <summary>
        /// Checks if a point lays on this plane.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>True if the point lays on the plane</returns>
        public bool LaysOnPlane(Vector2 point)
        {
            if (point.X == 2 && point.Y == 7)
                throw new Exception();

            return point.X >= UpperLeftCorner.X && point.X <= LowerRightCorner.X && point.Y >= LowerRightCorner.Y && point.Y <= UpperLeftCorner.Y;
        }

        /// <summary>
        /// Transforms plane coordiantes by one unit to specified direction.
        /// </summary>
        /// <param name="direction">Where to move</param>
        public Plane Move(Direction direction)
            => Move(direction.AsVector2());

        public Plane Move(Vector2 direction, float step)
        {
            direction *= step;
            UpperLeftCorner += direction;
            LowerRightCorner += direction;
            return this;
        }

        public Plane Move(Vector2 direction)
            => Move(direction, 1f);

        public void Move(Orientation2D direction, float step)
            => Move(direction.UnitVector, step);

        public void Move(Orientation2D direction)
            => Move(direction, 1f);

        /// <summary>
        /// Reduces the plane in given direction. Moves one side.
        /// </summary>
        /// <param name="direction"></param>
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

        public Plane ToAbsolute(Locality locality)
            => new Plane(GetAbsoluteCoordinates(UpperLeftCorner, locality), GetAbsoluteCoordinates(LowerRightCorner, locality));

        public Plane ToRelative()
                                                                                                                        => new Plane(GetRelativeCoordinates(UpperLeftCorner), GetRelativeCoordinates(LowerRightCorner));

        public override string ToString()
=> $"{UpperLeftCorner}, {LowerRightCorner}";
    }
}