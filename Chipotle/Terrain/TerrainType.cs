using Luky;

using System;

namespace Game.Terrain
{
    /// <summary>
    /// Defines types of terrain on the game map.
    /// </summary>
    public enum TerrainType : byte
    {
        /// <summary>
        /// Grass
        /// </summary>
        Grass = 0,

        /// <summary>
        /// Linoleum
        /// </summary>
        Linoleum = 1,

        /// <summary>
        /// Carpet
        /// </summary>
        Carpet = 2,

        /// <summary>
        /// Gravel
        /// </summary>
        Gravel = 3,

        /// <summary>
        /// Asphalt
        /// </summary>
        Asphalt = 4,

        /// <summary>
        /// Cobblestones
        /// </summary>
        Cobblestones = 5,

        /// <summary>
        /// Tiles
        /// </summary>
        Tiles = 6,

        /// <summary>
        /// Wood
        /// </summary>
        Wood = 7,

        /// <summary>
        /// Mud
        /// </summary>
        Mud = 8,

        /// <summary>
        /// Puddle
        /// </summary>
        Puddle = 9,

        /// <summary>
        /// Concrete
        /// </summary>
        Concrete = 10,

        /// <summary>
        /// Clay
        /// </summary>
        Clay = 11,

        /// <summary>
        /// Wall
        /// </summary>
        Wall = 12,

        /// <summary>
        /// Bush
        /// </summary>
        Bush = 13
    }

    public static class TerrainTypeExtension
    {
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