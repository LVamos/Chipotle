using Game.Terrain;

using System;

using UnityEngine;

namespace Game.PathFinding
{
	public class BoundingBox
	{
		/// <summary>
		/// Converts the box to a <see cref="Rectangle"/>. Note that <see cref="Rectangle"/> expects
		/// <c>UpperLeftCorner.y &gt;= LowerRightCorner.y</c> (Y grows downward there), the opposite of
		/// how <see cref="Min"/>/<see cref="Max"/> are named here - so the corners must be swapped on
		/// the Y axis, not passed straight through. Passing them straight through (as before) makes
		/// Rectangle.GetPoints()/Intersects() cover only a thin degenerate sliver of the box instead of
		/// its full area - PathFinder.BuildSnapshot relies on this being correct.
		/// </summary>
		public Rectangle ToRectangle()
			=> new(new Vector2(Min.x, Max.y), new Vector2(Max.x, Min.y));

		/// <summary>
		/// Calculates the bounding box for given start and goal positions with a margin.
		/// </summary>
		public static BoundingBox Create(Vector2 start, Vector2 goal, float margin)
		{
			Vector2 min = new(
				MathF.Min(start.x, goal.x) - margin,
				MathF.Min(start.y, goal.y) - margin
			);

			Vector2 max = new(
				MathF.Max(start.x, goal.x) + margin,
				MathF.Max(start.y, goal.y) + margin
			);

			return new BoundingBox(min, max);
		}

		public readonly Vector2 Min;
		public readonly Vector2 Max;

		public BoundingBox(Vector2 min, Vector2 max)
		{
			Min = min;
			Max = max;
		}
	}
}
