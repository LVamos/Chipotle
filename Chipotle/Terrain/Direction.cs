using Luky;

using System;
using System.Collections.Generic;

namespace Game.Terrain
{
    /// <summary>
    /// Possible directions for movement across the tile map.
    /// </summary>
    public enum Direction
    {
        None = 0,

        Right = 1,

        UpRight = 2,

        Up = 3,

        UpLeft = 4,

        Left = 5,

        DownLeft = 6,

        Down = 7,

        DownRight = 8,
    }

    [Flags]
    public enum Directions : byte
    {
        None = 0,
        Right = 1 << (Direction.Right - 1),
        UpRight = 1 << (Direction.UpRight - 1),
        Up = 1 << (Direction.Up - 1),
        UpLeft = 1 << (Direction.UpLeft - 1),
        Left = 1 << (Direction.Left - 1),
        DownLeft = 1 << (Direction.DownLeft - 1),
        Down = 1 << (Direction.Down - 1),
        DownRight = 1 << (Direction.DownRight - 1),

        All = Right | UpRight | Up | UpLeft | Left | DownLeft | Down | DownRight
    }





    public static class DirectionExtension

    {
        internal static readonly Vector2[] DirectionDeltas =
        {
new  Vector2(0, 0),
new  Vector2(1, 0),
new  Vector2(1, 1),
new  Vector2(0, 1),
new  Vector2(-1, 1),
new  Vector2(-1, 0),
new  Vector2(-1, -1),
new  Vector2(0, -1),

new  Vector2(1, -1),
    };

        public static Vector2 AsVector2(this Direction direction)
            => DirectionDeltas[(int)direction];

        public static bool IsVertical(this Direction d) => d == Direction.Up || d == Direction.Down;

        public static bool IsHorizontal(this Direction d) => d == Direction.Left || d == Direction.Right;

        public static Direction[] BasicDirections = new Direction[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down };

        public static bool IsBasic(this Direction direction)
            => Array.IndexOf(BasicDirections, direction) != -1;


        public static Direction GetOpposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Down: return Direction.Up;
                case Direction.Up: return Direction.Down;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                case Direction.DownLeft: return Direction.UpRight;
                case Direction.UpRight: return Direction.DownLeft;
                case Direction.UpLeft: return Direction.DownRight;
                case Direction.DownRight: return Direction.UpLeft;
                default: return Direction.None;
            }
        }
    }

    internal static class DirectionsExtensions
    {
        public static bool Includes(this Directions directions, Direction direction) => directions.HasFlag(direction.toDirections());

        private static Directions toDirections(this Direction direction) => (Directions)(1 << ((int)direction - 1));

        public static Directions And(this Directions directions, Direction direction) => directions | direction.toDirections();

        public static Directions Except(this Directions directions, Direction direction) => directions & ~direction.toDirections();

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

        public static IEnumerable<Direction> Enumerate(this Directions directions)
        {
            foreach (Direction direction in _directionList)
            {
                if (directions.Includes(direction))
                    yield return direction;
            }
        }


    }


}