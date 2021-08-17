using Game.Terrain;

using Luky;

namespace Game.Entities
{
    public class Corpse : DumpObject
    {
        public Corpse(Name name, Plane area) : base(name, area, "mrtvola", null, null, null, "cs5", true) { }

        private ChipotlesCar Car => World.GetObject("detektivovo auto") as ChipotlesCar;

        public override void Update()
        {
            base.Update();

            if (Car.Moved)
                Destroy();
        }
    }
}
