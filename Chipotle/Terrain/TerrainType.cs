using Luky;

using System;
using System.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// Defines types of 
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

        public static string GetDescription(this TerrainType terrain)
            => _descriptions[(int)terrain];

        public static string GetDescriptionInFourthCase(this TerrainType terrain)
    => _descriptions4Case[(int)terrain];



        public static string[] GetDescriptions()
            => _descriptions.ToArray<string>();

        /// <summary>
        /// Converts a string to TerrainType enumeration value. Accepts also czech descriptions.
        /// </summary>
        /// <param name="value">The string to parse</param>
        /// <returns></returns>
        public static TerrainType ToTerrainType(this string value)
        {
            string terrainDescription = value.PrepareForIndexing();
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

            return (TerrainType)Enum.Parse(typeof(TerrainType), value);
        }
    }


}
