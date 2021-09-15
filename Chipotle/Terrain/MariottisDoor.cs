using System.Collections.Generic;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    public class MariottisDoor : Door
    {
        public MariottisDoor(Name name, Plane area, IEnumerable<Locality> localities) : base(name, true, area, localities) { }

        protected override void Open(Vector2 coords)
            => World.PlayCutscene(this, "cs11");
    }
}