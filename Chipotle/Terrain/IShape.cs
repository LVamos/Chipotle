using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using System.Text;
using System.Threading.Tasks;

namespace Game.Terrain
{
    public interface IShape
    {
        IEnumerable<Vector2> GetPoints();
        IEnumerable<(Vector2 position, Tile tile)> GetTiles();
        bool Intersects(Rectangle area);
    }
}
