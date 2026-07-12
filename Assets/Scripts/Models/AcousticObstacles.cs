using Game.Terrain;

using System;
using System.Collections.Generic;

namespace Game.Models
{
    public class AcousticObstacles
    {
        public void UnionWith(AcousticObstacles other)
        {
            Objects.UnionWith(other.Objects);
            Tiles.UnionWith(other.Tiles);
        }

        public HashSet<MapElement> Objects { get; set; }
        public HashSet<TileInfo> Tiles { get; set; }

        public AcousticObstacles(
            HashSet<MapElement> objects,
            HashSet<TileInfo> tiles
            )
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            if (tiles == null)
            {
                throw new ArgumentNullException(nameof(tiles));
            }

            Objects = objects;
            Tiles = tiles;
        }
    }
}
