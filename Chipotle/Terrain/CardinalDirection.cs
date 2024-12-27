using System.Collections.Generic;

namespace Game.Terrain
{
    /// <summary>
    /// Enumerates cardinal directions.
    /// </summary>
    public enum CardinalDirection
    {
        /// <summary>
        /// North
        /// </summary>
        North = 0,

        /// <summary>
        /// North east
        /// </summary>
        NorthEast = 45,

        /// <summary>
        /// East
        /// </summary>
        East = 90,

        /// <summary>
        /// South east
        /// </summary>
        SouthEast = 135,

        /// <summary>
        /// South
        /// </summary>
        South = 180,

        /// <summary>
        /// South vest
        /// </summary>
        SouthWest = 225,

        /// <summary>
        /// Vest
        /// </summary>
        West = 270,

        /// <summary>
        /// North vest
        /// </summary>
        NorthWest = 315
    }

    /// <summary>
    /// Provides methods for conversion.
    /// </summary>
    public static class CardinalDirectionExtension
    {
        /// <summary>
        /// Text descriptions for cardianl directions
        /// </summary>
        private static readonly Dictionary<CardinalDirection, string> CardinalDirectionDescriptions = new Dictionary<CardinalDirection, string>()
        {
            [CardinalDirection.North] = "sever",
            [CardinalDirection.NorthEast] = "severovýchod",
            [CardinalDirection.East] = "východ",
            [CardinalDirection.SouthEast] = "jihovýchod",
            [CardinalDirection.South] = "jih",
            [CardinalDirection.SouthWest] = "jihozápad",
            [CardinalDirection.West] = "západ",
            [CardinalDirection.NorthWest] = "severozápad"
        };

        /// <summary>
        /// An extension method to convert a cardianl direction to text description
        /// </summary>
        /// <param name="direction">The cardinal direction to be converted</param>
        /// <returns>Text description of the cardinal direction</returns>
        public static string GetDescription(this CardinalDirection direction)
=> CardinalDirectionDescriptions[direction];
    }
}