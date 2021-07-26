using System.Collections.Generic;

namespace Game.Terrain
{
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


    public static class CardinalDirectionExtension
    {
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

        public static string GetDescription(this CardinalDirection direction)
=> CardinalDirectionDescriptions[direction];
    }





}
