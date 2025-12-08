using System;


using UnityEngine;

namespace Game.PathFinding
{
	public class BoundingBox
	{
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
