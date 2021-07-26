using Luky;

using System;

namespace Game.Terrain
{

    /// <summary>
    /// Indikates if a location is inside a building or outside.
    /// </summary>
    public enum LocalityType
    {
        /// <summary>
        /// A room or corridor in a building
        /// </summary>
        Indoor = 0,
        /// <summary>
        /// An openair place like yard or meadow
        /// </summary>
        Outdoor = 1
    }

    public static class LocalityTypeExtensions
    {
        public static LocalityType ToLocalityType(this string value)
        {
            string type = value.PrepareForIndexing();

            if (type == "venkovní" || type == "outdoor")
            {
                return LocalityType.Outdoor;
            }

            if (type == "vnitřní" || type == "indoor")
            {
                return LocalityType.Indoor;
            }

            throw new ArgumentException(value);
        }


        public static string GetDescription(this LocalityType type)
=> type == LocalityType.Indoor ? "vnitřní" : "venkovní";


    }

}
