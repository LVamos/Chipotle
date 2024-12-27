using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Terrain
{
	/// <summary>
	/// All directions in which game objects and NPCs can move.
	/// </summary>
	public enum Direction
	{
		/// <summary>
		/// Default
		/// </summary>
		None = 0,

		/// <summary>
		/// Right
		/// </summary>
		Right = 1,

		/// <summary>
		/// Up right
		/// </summary>
		UpRight = 2,

		/// <summary>
		/// Up
		/// </summary>
		Up = 3,

		/// <summary>
		/// Up left
		/// </summary>
		UpLeft = 4,

		/// <summary>
		/// Left
		/// </summary>
		Left = 5,

		/// <summary>
		/// Down left
		/// </summary>
		DownLeft = 6,

		/// <summary>
		/// Down
		/// </summary>
		Down = 7,

		/// <summary>
		/// Down right
		/// </summary>
		DownRight = 8,
	}

	/// <summary>
	/// All possible direction combinations
	/// </summary>
	[Flags]
	public enum Directions : byte
	{
		/// <summary>
		/// Default
		/// </summary>
		None = 0,

		/// <summary>
		/// Right
		/// </summary>
		Right = 1 << (Direction.Right - 1),

		/// <summary>
		/// Up right
		/// </summary>
		UpRight = 1 << (Direction.UpRight - 1),

		/// <summary>
		/// Up
		/// </summary>
		Up = 1 << (Direction.Up - 1),

		/// <summary>
		/// Up left
		/// </summary>
		UpLeft = 1 << (Direction.UpLeft - 1),

		/// <summary>
		/// Left
		/// </summary>
		Left = 1 << (Direction.Left - 1),

		/// <summary>
		/// Down left
		/// </summary>
		DownLeft = 1 << (Direction.DownLeft - 1),

		/// <summary>
		/// Down
		/// </summary>
		Down = 1 << (Direction.Down - 1),

		/// <summary>
		/// Down right
		/// </summary>
		DownRight = 1 << (Direction.DownRight - 1),

		/// <summary>
		/// All
		/// </summary>
		All = Right | UpRight | Up | UpLeft | Left | DownLeft | Down | DownRight
	}

	/// <summary>
	/// Useful extension methods for manipulation with directions
	/// </summary>
	public static class DirectionExtension
	{
		/// <summary>
		/// All basic directions
		/// </summary>
		public static Direction[] BasicDirections = new Direction[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down };

		/// <summary>
		/// Directions as unit vectors
		/// </summary>
		internal static readonly Vector2[] DirectionDeltas =
		{
			new(0, 0),
			new(1, 0),
			new(1, 1),
			new(0, 1),
			new(-1, 1),
			new(-1, 0),
			new(-1, -1),
			new(0, -1),

			new(1, -1),
		};

		/// <summary>
		/// Converts a direction to an unit vector.
		/// </summary>
		/// <param name="direction">A direction to be converted</param>
		/// <returns>An unit vector</returns>
		public static Vector2 AsVector2(this Direction direction)
			=> DirectionDeltas[(int)direction];

		/// <summary>
		/// Returns a direction which is opposite to the specified direction.
		/// </summary>
		/// <param name="direction">The default direction</param>
		/// <returns>The opposite direction</returns>
		public static Direction GetOpposite(this Direction direction)
		{
			return direction switch
			{
				Direction.Down => Direction.Up,
				Direction.Up => Direction.Down,
				Direction.Left => Direction.Right,
				Direction.Right => Direction.Left,
				Direction.DownLeft => Direction.UpRight,
				Direction.UpRight => Direction.DownLeft,
				Direction.UpLeft => Direction.DownRight,
				Direction.DownRight => Direction.UpLeft,
				_ => Direction.None
			};
		}

		/// <summary>
		/// Checks if the specified <paramref name="direction"/> is basic.
		/// </summary>
		/// <param name="direction"></param>
		/// <returns>True if the specified <paramref name="direction"/> is basic</returns>
		public static bool IsBasic(this Direction direction)
			=> Array.IndexOf(BasicDirections, direction) != -1;

		/// <summary>
		/// Checks if the specified <paramref name="d"/> is horizontal.
		/// </summary>
		/// <param name="d"></param>
		/// <returns>True if the specified <paramref name="d"/> is horizontal</returns>
		public static bool IsHorizontal(this Direction d)
			=> d == Direction.Left || d == Direction.Right;

		/// <summary>
		/// Checks if the specified <paramref name="d"/> is vertical.
		/// </summary>
		/// <param name="d"></param>
		/// <returns>True if the specified <paramref name="d"/> is vertical</returns>
		public static bool IsVertical(this Direction d)
			=> d == Direction.Up || d == Direction.Down;
	}

	/// <summary>
	/// few methods for manipulation with direction combinations
	/// </summary>
	internal static class DirectionsExtensions
	{
		/// <summary>
		/// List of all directions
		/// </summary>
		private static readonly Direction[] _directionList =
		{
			Direction.Right,
			Direction.UpRight,
			Direction.Up,
			Direction.UpLeft,
			Direction.Left,
			Direction.DownLeft,
			Direction.Down,
			Direction.DownRight,
		};

		/// <summary>
		/// Adds a direction to a set of directions.
		/// </summary>
		/// <param name="directions">A set of directions to extend</param>
		/// <param name="direction">A direction to be conbined with the set of directions</param>
		/// <returns>The extended set of direction</returns>
		public static Directions And(this Directions directions, Direction direction)
			=> directions | direction.ToDirections();

		/// <summary>
		/// Lists all directions from a set of directions
		/// </summary>
		/// <param name="directions">A set of directions to be listed</param>
		/// <returns></returns>
		public static IEnumerable<Direction> Enumerate(this Directions directions)
		{
			foreach (Direction direction in _directionList)
			{
				if (directions.Includes(direction))
					yield return direction;
			}
		}

		/// <summary>
		/// Removes the <paramref name="direction"/> from the <paramref name="directions"/> set.
		/// </summary>
		/// <param name="directions">The set of directions to reduce</param>
		/// <param name="direction">
		/// The direction to be removed from the <paramref name="directions"/> set
		/// </param>
		/// <returns>The reduced set of directions</returns>
		public static Directions Except(this Directions directions, Direction direction)
			=> directions & ~direction.ToDirections();

		/// <summary>
		/// Checks if the specified <paramref name="direction"/> is included in the <paramref
		/// name="directions"/> set.
		/// </summary>
		/// <param name="directions">The set of directions to be checked</param>
		/// <param name="direction">The direction to be compared with the set of directions</param>
		/// <returns>
		/// If if the specified <paramref name="direction"/> is included in the <paramref
		/// name="directions"/> set
		/// </returns>
		public static bool Includes(this Directions directions, Direction direction) => directions.HasFlag(direction.ToDirections());

		private static Directions ToDirections(this Direction direction) => (Directions)(1 << ((int)direction - 1));
	}
}