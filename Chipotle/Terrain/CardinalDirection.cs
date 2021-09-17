using System.Collections.Generic;

namespace Game.Terrain
{
    /// <summary>
    /// Enumerates cardinal directions.
    /// </summary>
    public enum CardinalDirection
    {
        North = 0,
        NorthEast = 45,
        East = 90,
        SouthEast = 135,
        South = 180,
        SouthWest = 225,
        West = 270,
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
        private static Dictionary<CardinalDirection, string> CardinalDirectionDescriptions = new Dictionary<CardinalDirection, string>()
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
