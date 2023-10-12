using Luky;

using OpenTK;
using OpenTK.Graphics.ES11;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Terrain
{
/// <summary>
/// Represents a parellogram (rovnoběžník) shape.
/// </summary>
/// 
    public class Parallelogram: DebugSO, IShape
    {
        public override string ToString() => $"{A.X}, {A.Y}; {B.X}, {B.Y}; {C.X}, {C.Y}; {D.X}, {D.Y}";
        /// <summary>
        /// Enumerates all tiles intersecting with the parallelogram.
        /// </summary>
        /// <returns>all tiles in the parallelogram</returns>
        public IEnumerable<(Vector2 position, Tile tile)> GetTiles()
        {
            IEnumerable<(Vector2 position, Tile tile)> tiles = GetPoints()
.Select(p => (p, World.Map[p]));

            return tiles
.Where(p => p.tile != null);
        }



        public IEnumerable<Vector2> GetPoints()
        {
            // Determine the bounds of the parallelogram
            float minX = Math.Min(Math.Min(A.X, B.X), Math.Min(C.X, D.X));
            float maxX = Math.Max(Math.Max(A.X, B.X), Math.Max(C.X, D.X));
            float minY = Math.Min(Math.Min(A.Y, B.Y), Math.Min(C.Y, D.Y));
            float maxY = Math.Max(Math.Max(A.Y, B.Y), Math.Max(C.Y, D.Y));

            // Iterate over all points within the bounds and check if they belong to the parallelogram
            for (float x = minX; x <= maxX; x += 1)
            {
                for (float y = minY; y <= maxY; y += 1)
                {
                    Vector2 point = new Vector2(x, y);
                    if (Contains(point))
                    {
                        yield return point;
                    }
                }
            }
        }

        private bool Contains(Vector2 point)
        {
            // Vector calculation of whether a point is in a parallelogram
            Vector2 v0 = C - A;
            Vector2 v1 = B - A;
            Vector2 v2 = point - A;

            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return (u >= 0) && (v >= 0) && (u + v < 1);
        }

        public bool Intersects(Rectangle rectangle)
        {
            // We test whether any corner of the rectangle is in a parallelogram
            if (Contains(rectangle.UpperLeftCorner)) return true;
            if (Contains(new Vector2(rectangle.LowerRightCorner.X, rectangle.UpperLeftCorner.Y))) return true;
            if (Contains(rectangle.LowerRightCorner)) return true;
            if (Contains(new Vector2(rectangle.UpperLeftCorner.X, rectangle.LowerRightCorner.Y))) return true;
            return false;

            return true;
        }

        public Vector2 A { get; set; }
        public Vector2 B { get; set; }
        public Vector2 C { get; set; }
        public Vector2 D { get; set; }

        public Parallelogram(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }
    }
}
