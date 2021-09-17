using Luky;

using System;
using System.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// Defines types of terrain on the game map.
    /// </summary>
    public enum TerrainType : byte
    {
        Grass = 0,
        Linoleum = 1,
        Carpet = 2,
        Gravel = 3,
        Asphalt = 4,
        Cobblestones = 5,
        Tiles = 6,
        Wood = 7,
        Mud = 8,
        Puddle = 9,
        Concrete = 10,
        Clay = 11,
        Wall = 12,
        Bush = 13
    }

    public static class TerrainTypeExtension
    {
        /// <summary>
        /// Text descriptions of the terrain in fourth declension case
        /// </summary>
        private static readonly string[] _descriptions4Case =
        {
            "trávu",
        "lino",
        "koberec",
"štěrk",
            "asfalt",
"kočičí hlavy",
"dlaždice",
"dřevo",
"bláto",
"louži",
"beton",
"hlínu",
"zeď",
"křoví"
        };


        /// <summary>
        /// Text descripitons of the terrain types
        /// </summary>
        private static readonly string[] _descriptions =
        {
            "tráva",
        "lino",
        "koberec",
"štěrk",
            "asfalt",
"kočičí hlavy",
"dlaždice",
"dřevo",
"bláto",
"louže",
"beton",
"hlína",
"zeď",
"Křoví"
        };

        /// <summary>
        /// Returns text description of the specified terrain type.
        /// </summary>
        /// <param name="terrain">The terrain to be described</param>
        /// <returns>text description of the specified terrain type</returns>
        public static string GetDescription(this TerrainType terrain)
            => _descriptions[(int)terrain];

        /// <summary>
        /// Returns text description of the specified terrain type in fourth declension case.
        /// </summary>
        /// <param name="terrain">The terrain to be described</param>
        /// <returns>text description of the specified terrain type in fourth declension case</returns>
        public static string GetDescriptionInFourthCase(this TerrainType terrain)
    => _descriptions4Case[(int)terrain];

        /// <summary>
        /// Parses the terrain type from the specified string.
        /// </summary>
        /// <param name="s">The string to be parsed</param>
        /// <returns>The terrain type enum</returns>
        public static TerrainType ToTerrainType(this string s)
        {
            string terrainDescription = s.PrepareForIndexing();
            int index = Array.IndexOf(_descriptions, terrainDescription);
            int index2 = Array.IndexOf(_descriptions4Case, terrainDescription);

            if (index != -1)
            {
                return (TerrainType)index;
            }

            if (index2 != -1)
            {
                return (TerrainType)index2;
            }

            return (TerrainType)Enum.Parse(typeof(TerrainType), s);
        }
    }


}
