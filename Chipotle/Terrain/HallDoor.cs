using Luky;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Terrain
{
    public class HallDoor : Door
    {


        public HallDoor(Name name, Plane area, IEnumerable<Locality> localities) : base(name, true, area, localities)
        {
        }

        protected override void Open(Vector2 coords)
        {
            if (!World.GetObject("lavička w1").Used)
                return;

            base.Open(coords);
        }
    }
}
