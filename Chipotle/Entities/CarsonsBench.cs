using Game.Messaging;
using Game.Messaging.Commands;
using Game.Terrain;
using Game.Entities;

using Luky;

using System.Linq;

namespace Game.Entities
{
    public class CarsonsBench : DumpObject
    {
        public CarsonsBench(Name name, Plane area) : base(name, area, "lavice u Carsona", null, null, null, "cs32", true)
        { }

        private ChipotlesCar Car  => World.GetObject("detektivovo auto") as ChipotlesCar;

        protected override void OnUseObject(UseObject message)
        {
            if (
                !World.GetObjectsByType("lavice u Carsona")
                .Any(o => o.Used)
                )
            {
                base.OnUseObject(message);
                Car.ReceiveMessage(new UnblockLocality(this, World.GetLocality("ulice v1")));
            }
        }
    }
}
