using Luky;

using System;

namespace Game.Terrain
{

    /// <summary>
    /// Indicates if a locality is inside a building or outside.
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

    /// <summary>
    /// A few methods for manipulation with the <see cref="LocalityType"/> enum
    /// </summary>
    public static class LocalityTypeExtensions
    {
        /// <summary>
        /// Parses a <see cref="LocalityType"/> from the specified string.
        /// </summary>
        /// <param name="s">The string to be parsed</param>
        /// <returns>A <see cref="LocalityType"/> enum</returns>
        public static LocalityType ToLocalityType(this string s)
        {
            string type = s.PrepareForIndexing();

            if (type == "venkovní" || type == "outdoor")
            {
                return LocalityType.Outdoor;
            }

            if (type == "vnitřní" || type == "indoor")
            {
                return LocalityType.Indoor;
            }

            throw new ArgumentException(s);
        }

        /// <summary>
        /// Returns a text description of the specified <see cref="LocalityType"/>.
        /// </summary>
        /// <param name="type">A <see cref="LocalityType"/> enum</param>
        /// <returns>The text description</returns>
        public static string GetDescription(this LocalityType type)
=> type == LocalityType.Indoor ? "vnitřní" : "venkovní";


    }

}
