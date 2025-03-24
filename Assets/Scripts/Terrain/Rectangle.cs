using Game.Entities;
using Game.Entities.Characters;
using Game.Models;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using UnityEngine;

namespace Game.Terrain
{
	/// <summary>
	/// Represents a movable resizable rectangle region of the game map.
	/// </summary>
	/// <remarks>
	/// The region is defined by two points: <see cref="Rectangle.UpperLeftCorner"/> and <see cref="Rectangle.LowerRightCorner"/>.
	/// </remarks>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public struct Rectangle
	{

		private float Left => UpperLeftCorner.x;
		private float Right => LowerRightCorner.x;
		private float Top => UpperLeftCorner.y;
		private float bottom => LowerRightCorner.y;

		/// <summary>
		/// Computes a point at the rectangle.
		/// </summary>
		/// <param name="direction">The direction vector of the wanted point (must be normalized).</param>
		/// <param name="distance">The distance to extend beyond the rectangle's front edge.</param>
		/// <returns>A point at the rectangle, at the specified distance from its front edge.</returns>
		public Vector2 GetPointInFront(Vector2 direction, float distance)
		{
			// Compute t for vertical boundaries:
			float tVertical = float.PositiveInfinity;
			if (direction.x > 0)
				tVertical = (Right - Center.x) / direction.x;
			else if (direction.x < 0)
				tVertical = (Left - Center.x) / direction.x;

			// Compute t for horizontal boundaries:
			float tHorizontal = float.PositiveInfinity;
			if (direction.y > 0)
				tHorizontal = (Top - Center.y) / direction.y;
			else if (direction.y < 0)
				tHorizontal = (bottom - Center.y) / direction.y;

			// Choose the smallest positive t (the intersection with the rectangle boundary)
			float t = Mathf.Min(tVertical, tHorizontal);

			// Calculate the boundary point in the given direction
			Vector2 boundaryPoint = Center + direction * t;

			// Extend from the boundary point by the given distance along the same direction
			Vector2 result = boundaryPoint + direction * distance;

			return result;
		}

		public float GetDistanceFrom(Rectangle other)
		{
			if (Intersects(other))
				return 0;

			float horizontalGap = 0f;
			if (this.LowerRightCorner.x < other.UpperLeftCorner.x)
				horizontalGap = other.UpperLeftCorner.x - this.LowerRightCorner.x;
			else if (other.LowerRightCorner.x < this.UpperLeftCorner.x)
				horizontalGap = this.UpperLeftCorner.x - other.LowerRightCorner.x;

			float verticalGap = 0f;
			if (this.UpperLeftCorner.y < other.LowerRightCorner.y)
				verticalGap = other.LowerRightCorner.y - this.UpperLeftCorner.y;
			else if (other.UpperLeftCorner.y < this.LowerRightCorner.y)
				verticalGap = this.LowerRightCorner.y - other.UpperLeftCorner.y;

			float distance = Mathf.Sqrt(horizontalGap * horizontalGap + verticalGap * verticalGap);
			return distance.Round();
		}

		/// <summary>
		/// Gets the distance from the center to a corner.
		/// </summary>
		public float DistanceFromCenterToCorner => World.GetDistance(UpperLeftCorner, Center);

		private Vector2 _upperLeftCorner;
		private Vector2 _lowerRightCorner;

		/// <summary>
		/// Finds the nearest point on a rectangle to a given point.
		/// </summary>
		/// <param name="point">The point to calculate the nearest point on the rectangle.</param>
		/// <returns>The nearest point on the rectangle to the given point.</returns>
		public Vector2 GetClosestPoint(Vector2 point)
		{
			// Snapping the X coordinate of the point to the range of X coordinates of the rectangle
			float clampedX = Clamp(point.x, UpperLeftCorner.x, LowerRightCorner.x);

			// Snapping the Y coordinate of the point to the Y coordinate range of the rectangle
			float clampedY = Clamp(point.y, UpperLeftCorner.y, LowerRightCorner.y);

			if (point.x >= LowerLeftCorner.x && point.x <= LowerRightCorner.x)
				return point.y <= LowerRightCorner.y ? Round(new(point.x, LowerRightCorner.y)) : Round(new(point.x, UpperRightCorner.y));

			if (point.y >= LowerLeftCorner.y && point.y <= UpperLeftCorner.y)
				return point.x <= LowerLeftCorner.x ? new(LowerLeftCorner.x, point.y) : new(LowerRightCorner.x, point.y);

			// Vypočítání vzdáleností od každé hrany
			float distanceToLeft = point.x - UpperLeftCorner.x;
			float distanceToRight = LowerRightCorner.x - point.x;
			float distanceToTop = UpperLeftCorner.y - point.y;
			float distanceToBottom = point.y - LowerRightCorner.y;

			// Určení nejbližší hrany
			float minDistance = Math.Min(Math.Min(distanceToLeft, distanceToRight), Math.Min(distanceToTop, distanceToBottom));

			if (minDistance == distanceToLeft)
				return new(UpperLeftCorner.x, clampedY);

			if (minDistance == distanceToRight)
				return new(LowerRightCorner.x, clampedY);

			if (minDistance == distanceToTop)
				return new(clampedX, UpperLeftCorner.y);

			if (minDistance == distanceToBottom)
				return new(clampedX, LowerRightCorner.y);

			// Pokud bod leží přesně v rohu, vrátíme upnutý bod
			return new(clampedX, clampedY);
		}

		/// <summary>
		/// Clamps the specified value between the specified minimum and maximum values.
		/// </summary>
		/// <param name="value">The value to clamp.</param>
		/// <param name="min">The minimum value to clamp to.</param>
		/// <param name="max">The maximum value to clamp to.</param>
		/// <returns>The clamped value.</returns>
		private float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value));

		/// <summary>
		/// Calculating a point on the perimeter of the rectangle in the given direction
		/// </summary>
		/// <param name="direction"></param>
		/// <returns>A point on perimeter</returns>
		public Vector2 PointOnPerimeter(Vector2 direction)
		{
			// Normalize the direction vector
			Vector2 normalizedDirection = direction.normalized;

			// Calculate width and height
			float width = LowerRightCorner.x - UpperLeftCorner.x;
			float height = UpperLeftCorner.y - LowerRightCorner.y; // Note: Y is inverted

			// Ratio of width to height of the rectangle
			float aspectRatio = width / height;

			// Point on the perimeter
			float x, y;

			if (Math.Abs(normalizedDirection.x) < Math.Abs(normalizedDirection.y * aspectRatio))
			{
				// Top or bottom side
				y = normalizedDirection.y < 0 ? LowerRightCorner.y : UpperLeftCorner.y;
				x = Center.x + normalizedDirection.x / Math.Abs(normalizedDirection.y) * height / 2;
			}
			else
			{
				// Left or right side
				x = normalizedDirection.x > 0 ? LowerRightCorner.x : UpperLeftCorner.x;
				y = Center.y - normalizedDirection.y / Math.Abs(normalizedDirection.x) * width / 2;
			}

			return new(x, y);
		}

		public Vector2 GetPointOnRectangleEdge(Vector2 direction)
		{
			// Normalize the direction vector
			Vector2 normalizedDirection = direction.normalized;

			// Calculate intersections with the rectangle
			Vector2[] edgePoints = new Vector2[4];
			edgePoints[0] = new(Center.x, UpperLeftCorner.y); // top
			edgePoints[1] = new(Center.x, LowerRightCorner.y); // bottom
			edgePoints[2] = new(UpperLeftCorner.x, Center.y); // left
			edgePoints[3] = new(LowerRightCorner.x, Center.y); // right

			// Find the point that is in the direction of the vector and closest to the center
			Vector2 closestPoint = edgePoints[0];
			float maxDot = float.NegativeInfinity;

			foreach (Vector2 point in edgePoints)
			{
				Vector2 toPoint = point - Center;
				float dot = Vector2.Dot(normalizedDirection, toPoint.normalized);
				if (dot > maxDot)
				{
					maxDot = dot;
					closestPoint = point;
				}
			}

			return closestPoint;
		}

		public IEnumerable<Vector2> Corners
		{
			get
			{
				yield return UpperLeftCorner;
				yield return UpperRightCorner;
				yield return LowerLeftCorner;
				yield return LowerRightCorner;
			}
		}

		/// <summary>
		/// Checks if any points are outside of the map.
		/// </summary>
		/// <returns>True if any points are outside of the map, otherwise false.</returns>
		public bool IsOutOfMap() => Corners.Any(p => World.GetLocality(p) == null);

		/// <summary>
		/// Checks if all the plane is walkable.
		/// </summary>
		public bool Walkable()
		{
			return
				GetPoints()
					.All(World.IsWalkable);
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
		/// Initializes a rectangle with a specified upper left corner, width, and height.
		/// </summary>
		/// <param name="upperLeft">The position of the upper left corner of the rectangle.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		public Rectangle(Vector2 upperLeft, float width, float height)
			: this(upperLeft, new(upperLeft.x + width, upperLeft.y - height)) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="upperLeft">Coordinates of the upper left corner of the rectangle</param>
		/// <param name="lowerRight">Coordinates of the lower right corner of the rectangle</param>
		public Rectangle(Vector2 upperLeft, Vector2 lowerRight)
		{
			MinimumHeight = MinimumWidth = 0;
			_upperLeftCorner = new(upperLeft.x.Round(), upperLeft.y.Round());
			_lowerRightCorner = new(lowerRight.x.Round(), lowerRight.y.Round());
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="upperLeft">Coordinates of the upper left corner of the rectangle</param>
		/// <remarks>It creates a square of size 1.</remarks>
		public Rectangle(Vector2 upperLeft) : this(upperLeft, upperLeft)
		{
		}

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="value">A string with four coordinates separated by comma</param>
		public Rectangle(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException($"{nameof(value)} cann't be null.");

			MinimumHeight = MinimumWidth = 0;

			List<float> coords = Parse(value);
			_upperLeftCorner = new Vector2(coords[0].Round(), coords[1].Round());
			_lowerRightCorner = coords.Count == 2 ? _upperLeftCorner : new(coords[2].Round(), coords[3].Round());
		}

		private static List<float> Parse(string value)
		{
			// Parse the values
			List<float> coords = new();
			string trimmed = value.Trim();

			string[] split = trimmed.Split(',');
			foreach (string coordinate in split)
				coords.Add(float.Parse(coordinate.Trim(), CultureInfo.InvariantCulture));

			// Check format
			return coords.Count is not 4 and not 2 ? throw new InvalidOperationException($"{nameof(value)} in invalid format") : coords;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="plane">Another plane to be copied</param>
		public Rectangle(Rectangle plane)
		{
			_upperLeftCorner = plane.UpperLeftCorner;
			_lowerRightCorner = plane.LowerRightCorner;
			MinimumHeight = plane.MinimumHeight;
			MinimumWidth = plane.MinimumWidth;
		}

		public Rectangle Rotate90() => FromCenter(Center, Width, Height);

		public static Rectangle FromCenter(Vector2 center, float height, float width)
		{
			float halfWidth = width / 2;
			float halfHeight = height / 2;

			Vector2 upperLeftCorner = new(center.x - halfWidth, center.y + halfHeight);
			Vector2 lowerRightCorner = new(center.x + halfWidth, center.y - halfHeight);
			return new(upperLeftCorner, lowerRightCorner);
		}

		/// <summary>
		/// Returns coordinates of the center of the plane.
		/// </summary>
		[ProtoIgnore]
		public Vector2 Center
		{
			get
			{
				Vector2 result = new(
					(UpperLeftCorner.x + LowerRightCorner.x) / 2,
					(UpperLeftCorner.y + LowerRightCorner.y) / 2);

				return Round(result);
			}
		}

		/// <summary>
		/// Height of the plane.
		/// </summary>
		[ProtoIgnore]
		public float Height => (UpperLeftCorner.y - LowerRightCorner.y).Round();

		/// <summary>
		/// Returns coordinates of the lower left corner of the plane.
		/// </summary>
		[ProtoIgnore]
		public Vector2 LowerLeftCorner
			=> new(UpperLeftCorner.x, LowerRightCorner.y);

		/// <summary>
		/// Gets or sets the lower right corner of the Rectangle.
		/// </summary>
		public Vector2 LowerRightCorner
		{
			get => _lowerRightCorner;
			set => _lowerRightCorner = Round(value);
		}

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
		public float Size => Height * Width;

		private Vector2 Round(Vector2 coordinates)
		{
			return new(
						coordinates.x.Round(),
						coordinates.y.Round());
		}

		/// <summary>
		/// Coordinates of the upper left corner of the plane.
		/// </summary>
		public Vector2 UpperLeftCorner
		{
			get => _upperLeftCorner;
			set => _upperLeftCorner = Round(value);
		}

		/// <summary>
		/// Coordinates of the upper right corner of the plane.
		/// </summary>
		[ProtoIgnore]
		public Vector2 UpperRightCorner => new(LowerRightCorner.x, UpperLeftCorner.y);

		/// <summary>
		/// Returns width of the plane.
		/// </summary>
		public float Width
			=> (LowerRightCorner.x - UpperLeftCorner.x).Round();

		/// <summary>
		/// Converts relative coordinates to absolute coordinates.
		/// </summary>
		/// <param name="relative">The coordinates related to upper left corner of <paramref name="area"/></param>
		/// <param name="area">
		/// The locality according to which <paramref name="relative"/> was calculated
		/// </param>
		/// <returns>New isntance of <see cref="Rectangle"/> defined by the absolute coordinates</returns>
		public static Vector2 GetAbsoluteCoordinates(Vector2 relative, Rectangle area) => new(area.UpperLeftCorner.x + relative.x, area.UpperLeftCorner.y - relative.y);

		/// <summary>
		/// Converts absolute coordinates to relative coordinates.
		/// </summary>
		/// <param name="absolute">Absolute coordiantes to be converted</param>
		/// <returns></returns>
		public static Vector2 GetRelativeCoordinates(Vector2 absolute)
		{
			Rectangle locality = World.GetLocality(absolute).Area.Value;
			locality.ArrangeCorners();
			Vector2 corner = locality.UpperLeftCorner;
			return new(absolute.x - corner.x, corner.y - absolute.y);
		}

		/// <summary>
		/// Verifies that <see cref="UpperLeftCorner"/> points more to the left and more upper than
		/// the <see cref="LowerRightCorner"/> and rearranges them if necessary.
		/// </summary>
		public void ArrangeCorners()
		{
			float temp;
			if (UpperLeftCorner.x > LowerRightCorner.x)
			{ // Swap them
				temp = UpperLeftCorner.x;
				UpperLeftCorner = new(LowerRightCorner.x, UpperLeftCorner.y);
				LowerRightCorner = new(temp, LowerRightCorner.y);
			}

			if (UpperLeftCorner.y < LowerRightCorner.y)
			{ // Swap them
				temp = UpperLeftCorner.y;
				UpperLeftCorner = new(UpperLeftCorner.x, LowerRightCorner.y);
				LowerRightCorner = new(LowerRightCorner.x, temp);
			}
		}

		/// <summary>
		/// Checks if the given plane fully intersects this plane.
		/// </summary>
		/// <param name="area">The plane to be checked</param>
		/// <returns>True if the given plane fully intersects this plane</returns>
		public bool Contains(Rectangle area) => Contains(area.UpperLeftCorner) && Contains(area.LowerRightCorner);

		/// <summary>
		/// Checks if both instances are equal.
		/// </summary>
		/// <param name="p">The other plane to compare</param>
		/// <returns>True if both instances are equal</returns>
		public bool Equals(Rectangle p) => UpperLeftCorner == p.UpperLeftCorner && LowerRightCorner == p.LowerRightCorner;

		/// <summary>
		/// Extends the plane to all directions by the specified amount of units in all directions.
		/// </summary>
		/// <param name="units">Amount of the units by which the rectangle is extended</param>
		public void Extend(float distance = 1)
		{
			UpperLeftCorner = new Vector2(
				UpperLeftCorner.x - distance,
				UpperLeftCorner.y + distance
			);

			LowerRightCorner = new Vector2(
				LowerRightCorner.x + distance,
				LowerRightCorner.y - distance
			);
		}

		/// <summary>
		/// Extends the plane in the specified direction by one unit.
		/// </summary>
		/// <param name="direction">Side of the plane to be moved</param>
		public void Extend(Direction direction)
		{
			Vector2 step = direction.AsVector2();
			if (direction is Direction.Left or Direction.Up)
				UpperLeftCorner += step;
			else if (direction is Direction.Right or Direction.Down)
				LowerRightCorner += step;
		}

		/// <summary>
		/// Returns distance between the plane and the default point.
		/// </summary>
		/// <param name="point">The point from which the distance is to be calculated</param>
		/// <returns>The distance between the default point and the plane</returns>
		public float GetDistanceFrom(Vector2 point)
		{
			Vector2 closestPoint = GetClosestPoint(point);
			return World.GetDistance(closestPoint, point);
		}

		/// <summary>
		/// Returns hash code of this instance.
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode() => unchecked(4112 * (8121 + UpperLeftCorner.GetHashCode()) * (6988 + LowerRightCorner.GetHashCode()));

		/// <summary>
		/// Enumerates entities intersecting with the plane.
		/// </summary>
		/// <returns>Enumeration of intersecting entities</returns>
		public IEnumerable<Character> GetEntities() => World.GetCharacters(this);

		/// <summary>
		/// Returns all game objects intersecting with the plane.
		/// </summary>
		/// <returns>List of intersecting objects</returns>
		public IEnumerable<Entity> GetObjects() => World.GetItems(this);

		/// <summary>
		/// Returns all passages intersecting with the plane.
		/// </summary>
		/// <returns>List of intersecting passages</returns>
		public IEnumerable<Passage> GetPassages() => World.GetPassages(this);

		/// <summary>
		/// Identifies a perimeter side of the plane on which the specified point lays.
		/// </summary>
		/// <param name="point">The point to be checked</param>
		/// <returns>
		/// A <see cref="Direction"/> enum identifying the side on which the specified point lays.
		/// </returns>
		public Direction GetIntersectingSide(Vector2 point)
		{
			Rectangle copy = this;
			return
			DirectionExtension.BasicDirections
				.Where(side => copy.LaysOnSide(side, point))
				.FirstOrDefault();
		}

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
			return
				GetPerimeterPoints()
					.Select(p => World.Map[p])
					.Where(p => p != null);
		}

		/// <summary>
		/// Enumerates all point laying on the perimeter of this plane.
		/// </summary>
		/// <returns>all point laying on the perimeter of this plane</returns>
		public IEnumerable<Vector2> GetPerimeterPoints(float resolution = .1f)
		{
			return GetPerimeterPoints(Direction.Left, resolution)
						.Union(GetPerimeterPoints(Direction.Up, resolution))
						.Union(GetPerimeterPoints(Direction.Right, resolution))
						.Union(GetPerimeterPoints(Direction.Down, resolution));
		}

		/// <summary>
		/// Enumerates the perimeter points lying on the specified sides of this plane.
		/// </summary>
		/// <param name="sides">Perimeter sides whose points are to be returned</param>
		/// <returns>the perimeter points lying on the specified sides of this plane</returns>
		public IEnumerable<Vector2> GetPerimeterPoints(IEnumerable<Direction> sides, float resolution = .1f)
		{
			Rectangle copy = this;
			return
			sides.Select(side => copy.GetPerimeterPoints(side, resolution))
				.SelectMany(p => p)
				.Distinct();
		}

		/// <summary>
		/// Enumerates the perimeter points laying on the specified side.
		/// </summary>
		/// <param name="side">The perimeter side whose points are to be enumerated</param>
		/// <returns>the perimeter points laying on the specified side</returns>
		public IEnumerable<Vector2> GetPerimeterPoints(Direction side, float resolution = .1f)
		{
			float x, y;
			float left = UpperLeftCorner.x;
			float right = LowerRightCorner.x;
			float top = UpperLeftCorner.y;
			float bottom = LowerRightCorner.y;

			switch (side)
			{
				case Direction.Left:
					for (y = bottom; y <= top; y = (y + resolution).Round())
						yield return new(left, y);
					break;

				case Direction.Right:
					for (y = bottom; y <= top; y = (y + resolution).Round())
						yield return new(right, y);
					break;

				case Direction.Up:
					for (x = left; x <= right; x = (x + resolution).Round())
						yield return new(x, top);
					break;

				case Direction.Down:
					for (x = left; x <= right; x = (x + resolution).Round())
						yield return new(x, bottom);
					break;

				default:
					throw new ArgumentException(nameof(side));
			}
		}

		/// <summary>
		/// Enumerates all tiles laying on the perimeter of this plane.
		/// </summary>
		/// <returns>all tiles laying on the perimeter of this plane</returns>
		public IEnumerable<(Vector2 position, Tile tile)> GetPerimeterTiles(float resolution = .1f)
		{
			IEnumerable<(Vector2 position, Tile tile)> points = GetPerimeterPoints(resolution)
				.Select(p => (p, World.Map[p]));
			return points.Where(t => t.tile != null);
		}

		/// <summary>
		/// Enumerates all tiles laying on the specified side of this plane.
		/// </summary>
		/// <param name="side">Side of the plane to be enumerated</param>
		/// <returns>all tiles laying on the specified side of this plane</returns>
		public List<TileInfo> GetPerimeterTiles(Direction side, float resolution = .1f)
		{
			IEnumerable<Vector2> points = GetPerimeterPoints(side, resolution);
			List<TileInfo> tiles = new();

			foreach (Vector2 position in points)
			{
				Tile tile = World.Map[position];
				if (tile != null)
					tiles.Add(new(position, tile));
			}

			return tiles;
		}

		/// <summary>
		/// Enumerates all points of the plane.
		/// </summary>
		/// <returns>all points of the plane</returns>
		public IEnumerable<Vector2> GetPoints(float resolution = 0.1f)
		{
			Vector2 upperLeft = World.Map.SnapToGrid(UpperLeftCorner);
			Vector2 upperRight = World.Map.SnapToGrid(UpperRightCorner);
			Vector2 lowerRight = World.Map.SnapToGrid(LowerRightCorner);

			float startX = upperLeft.x;
			float endX = lowerRight.x;
			float startY = lowerRight.y;
			float endY = upperRight.y;

			int stepsX = (int)Math.Floor((endX - startX) / resolution) + 1;
			int stepsY = (int)Math.Floor((endY - startY) / resolution) + 1;

			for (int i = 0; i < stepsX; i++)
			{
				float x = startX + i * resolution;
				if (x > endX)
					x = endX;

				for (int j = 0; j < stepsY; j++)
				{
					float y = startY + j * resolution;
					if (y > endY)
						y = endY;

					float roundedX = (float)Math.Round(x, 1, MidpointRounding.AwayFromZero);
					float roundedY = (float)Math.Round(y, 1, MidpointRounding.AwayFromZero);

					yield return new Vector2(roundedX, roundedY);
				}
			}
		}

		/// <summary>
		/// Enumerates all points sorted by distance from the default point.
		/// </summary>
		/// <param name="point">Coordinates of the default point whose surroundings should be explored.</param>
		/// <returns>Coordinates</returns>
		public IEnumerable<Vector2> GetPointsByDistance(Vector2 point) => GetPoints().OrderBy(p => World.GetDistance(p, point));

		/// <summary>
		/// Returns a new plane corresponding to coordinates of the specified perimeter side of this plane.
		/// </summary>
		/// <param name="side">The side of this plane to be copied</param>
		/// <returns>A new plane corresponding to the specified perimeter side of this plane</returns>
		public Rectangle GetSide(Direction side)
		{
			return side switch
			{
				Direction.Left => new(UpperLeftCorner, LowerLeftCorner),
				Direction.Up => new(UpperLeftCorner, UpperRightCorner),
				Direction.Right => new(UpperRightCorner, LowerRightCorner),
				Direction.Down => new(LowerLeftCorner, LowerRightCorner),
				_ => throw new ArgumentException("Illegal direction")
			};
		}

		/// <summary>
		/// Returns two points that define the specified perimeter side of this plane.
		/// </summary>
		/// <param name="side">The side to be copied</param>
		/// <returns>A tuple with two points defining the specified perimeter side of this plane</returns>
		public (Vector2 Point1, Vector2 Point2) GetSideDefinition(Direction side)
		{
			return side switch
			{
				Direction.Left => (LowerLeftCorner, UpperLeftCorner),
				Direction.Up => (UpperLeftCorner, UpperRightCorner),
				Direction.Right => (LowerRightCorner, UpperRightCorner),
				Direction.Down => (LowerLeftCorner, LowerRightCorner),
				_ => throw new ArgumentException(nameof(side))
			};
		}

		/// <summary>
		/// Enumerates all closest points from surroundings of this plane.
		/// </summary>
		/// <param name="minDistance">Specifies minimum distance of the surrounding points from edges of the specified plane.</param>
		/// <param name="maxDistance">Specifies maximum distance of the surrounding points from edges of the specified plane.</param>
		/// <returns>all closest points from surroundings of this plane</returns>
		public IEnumerable<Vector2> GetSurroundingPoints(int minDistance, int maxDistance, float tileSize = .1f)
		{
			Rectangle maxRectangle = this;
			maxRectangle.Extend(maxDistance);
			Rectangle minRectangle = this;
			minRectangle.Extend(minDistance);
			IEnumerable<Vector2> points = maxRectangle.GetPoints(tileSize);

			return points
.Where(point => !minRectangle.Contains(point));
		}

		/// <summary>
		/// Finds a point on the rectangle that is alligned to the specified point.
		/// </summary>
		/// <param name="point">The point to be aligned</param></param>
		public Vector2? FindAlignedPoint(Vector2 point)
		{
			// Check if the point is aligned vertically
			if (point.x >= this.UpperLeftCorner.x && point.x <= this.LowerRightCorner.x)
			{
				if (point.y > this.UpperLeftCorner.y)
					return new Vector2(point.x, this.UpperLeftCorner.y); // Above the rectangle
				else if (point.y < this.LowerRightCorner.y)
					return new Vector2(point.x, this.LowerRightCorner.y); // Below the rectangle
			}

			// Check if the point is aligned horizontally
			if (point.y >= this.LowerRightCorner.y && point.y <= this.UpperLeftCorner.y)
			{
				if (point.x < this.UpperLeftCorner.x)
					return new Vector2(this.UpperLeftCorner.x, point.y); // Left of the rectangle
				else if (point.x > this.LowerRightCorner.x)
					return new Vector2(this.LowerRightCorner.x, point.y); // Right of the rectangle
			}

			// the point is not aligned
			return null;
		}

		/// <summary>
		/// Enumerates all closest tiles from surroundings of this plane.
		/// </summary>
		/// <param name="minDistance">Minimum distance of the surrounding points from the plane</param>
		/// <param name="maxDistance">Maximum distance of the surrounding tiles from the plane</param>
		/// <returns>all closest tiles from surroundings of this plane</returns>
		public List<TileInfo> GetSurroundingTiles(int minDistance, int maxDistance)
		{
			IEnumerable<Vector2> points = GetSurroundingPoints(minDistance, maxDistance);
			List<TileInfo> tiles = new();

			foreach (Vector2 point in points)
			{
				Tile tile = World.Map[point];
				if (tile != null)
					tiles.Add(new(point, tile));
			}

			return tiles;
		}

		/// <summary>
		/// Retrieves all the points that are located outside the map.
		/// </summary>
		/// <returns>An enumerable collection of Vector2 points.</returns>
		public IEnumerable<Vector2> GetPointsOutsideMap()
		{
			return GetPoints()
				.Where(p => World.Map[p] == null);
		}

		/// <summary>
		/// Enumerates all tiles intersecting with the plane.
		/// </summary>
		/// <returns>all tiles intersecting with the plane</returns>
		public List<TileInfo> GetTiles(float resolution = .1f)
		{
			IEnumerable<Vector2> points = GetPoints(resolution);

			List<TileInfo> tiles = new();
			foreach (Vector2 point in points)
			{
				Tile tile = World.Map[point];
				if (tile != null)
					tiles.Add(new(point, tile));
			}

			return tiles;
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
		public IEnumerable<TileInfo> GetWalkableTiles()
		{
			return GetTiles()
						.Where(t => World.IsWalkable(t.Position));
		}

		/// <summary>
		/// Checks if this plane intersects with the specified plane.
		/// </summary>
		/// <param name="plane">The plane to be compared</param>
		/// <returns>True if the planes intersect with each other</returns>
		public bool Intersects(Rectangle plane)
		{
			return !(plane.UpperLeftCorner.x > this.LowerRightCorner.x || // plane is to the right of this
					 plane.LowerRightCorner.x < this.UpperLeftCorner.x || // plane is to the left of this
					 plane.UpperLeftCorner.y < this.LowerRightCorner.y || // plane is below this
					 plane.LowerRightCorner.y > this.UpperLeftCorner.y);  // plane is above this
		}

		/// <summary>
		/// Checks if the specified point lays on the specified perimeter side of this plane.
		/// </summary>
		/// <param name="side">The perimeter side to be checked</param>
		/// <param name="point">The point to be checked</param>
		/// <returns>if the specified point lays on the specified perimeter side of this plane</returns>
		public bool LaysOnSide(Direction side, Vector2 point)
		{
			(Vector2 Point1, Vector2 Point2) = GetSideDefinition(side);
			return point.x >= Point1.x && point.y >= Point1.y && point.x <= Point2.x && point.y <= Point2.y;
		}

		/// <summary>
		/// Checks if the specified point lays on the periemter of this plane.
		/// </summary>
		/// <param name="point">The point to be checked</param>
		/// <returns>if the specified point lays on the periemter of this plane</returns>
		public bool IsOnPerimeter(Vector2 point)
		{
			Rectangle copy = this;
			return
			DirectionExtension.BasicDirections
				.Any(side => copy.LaysOnSide(side, point));
		}

		/// <summary>
		/// Checks if the specified point lays on this plane.
		/// </summary>
		/// <param name="point">The point to be checked</param>
		/// <returns>True if the point lays on the plane</returns>
		public bool Contains(Vector2 point) => point.x >= UpperLeftCorner.x && point.x <= LowerRightCorner.x && point.y >= LowerRightCorner.y && point.y <= UpperLeftCorner.y;

		/// <summary>
		/// Transforms plane coordinates by one unit to the specified direction.
		/// </summary>
		/// <param name="direction">The direction where the plane is to be moved</param>
		public Rectangle Move(Direction direction) => Move(direction.AsVector2());

		/// <summary>
		/// Transforms plane coordinates to the specified direction.
		/// </summary>
		/// <param name="direction">
		/// The direction defined by a unit vector where the plane is to be moved
		/// </param>
		/// <param name="step">Specifies length of the transformation</param>
		public Rectangle Move(Vector2 direction, float step)
		{
			direction = Round(direction * step);
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
		public Rectangle Move(Vector2 direction) => Move(direction, 1f);

		/// <summary>
		/// Transforms plane coordinates to the specified direction.
		/// </summary>
		/// <param name="direction">
		/// The direction defined by an <see cref="Orientation2D"/> struct where the plane is to be moved
		/// </param>
		/// <param name="step">Specifies length of the transformation</param>
		public void Move(Orientation2D direction, float step) => Move(direction.UnitVector, step);

		/// <summary>
		/// Transforms plane coordinates one unit to the specified direction.
		/// </summary>
		/// <param name="direction">
		/// The direction defined by an <see cref="Orientation2D"/> struct where the plane is to be moved
		/// </param>
		public void Move(Orientation2D direction) => Move(direction, 1f);

		/// <summary>
		/// Reduces the plane in given direction. Moves one side.
		/// </summary>
		/// <param name="direction">Direction of the reduction</param>
		public void Reduce(Direction direction)
		{
			if ((direction.IsVertical() && Height > MinimumHeight) || (direction.IsHorizontal() && Width < MinimumWidth))
				throw new InvalidOperationException("Minimum size exceeded.");

			if (((direction == Direction.Left || direction == Direction.Right) && Width == 1) || ((direction == Direction.Up || direction == Direction.Down) && Height == 1))
				return;

			Vector2 step = direction.GetOpposite().AsVector2();
			if (direction is Direction.Left or Direction.Up)
				UpperLeftCorner += step;
			else if (direction is Direction.Right or Direction.Down)
				LowerRightCorner += step;
		}

		/// <summary>
		/// Converts the plane to a plane with absolute coordinations.
		/// </summary>
		/// <param name="area">The plane according to which coordinates of this plane were calculated</param>
		/// <returns>A new plane with absolute coordinates</returns>
		/// <remarks>Considers this plane to be relative</remarks>
		public Rectangle ToAbsolute(Rectangle area) => new(GetAbsoluteCoordinates(UpperLeftCorner, area), GetAbsoluteCoordinates(LowerRightCorner, area));

		/// <summary>
		/// converts this plane to a plane with relative coordinates.
		/// </summary>
		/// <returns>A new plane with relative coordinates</returns>
		/// <remarks>Considers this plane to be absolute</remarks>
		public Rectangle ToRelative() => new(GetRelativeCoordinates(UpperLeftCorner), GetRelativeCoordinates(LowerRightCorner));

		/// <summary>
		/// Returns coordinates of the plane as a string.
		/// </summary>
		/// <returns>coordinates of the plane as a comma separated string</returns>
		public override string ToString() => $"{UpperLeftCorner}, {LowerRightCorner}";
	}
}