
using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using System.Linq;

namespace Game.Entities
{
    public class IcecreamMachine : DumpObject
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Name of the machine</param>
        /// <param name="area">Location of the machine</param>
        public IcecreamMachine(Name name, Plane area) : base(name, area, "automat na zmrzlinu", null, null, "VendingMachineLoop")
        { }

        protected override void OnUseObject(UseObject message)
        {
            Entity tuttle = World.GetEntity("tuttle");
            Locality garage = World.GetLocality("garáž v1");
            if (
                Locality.IsItHere(tuttle)
                && tuttle.VisitedLocalities.Contains(garage)
                && World.Player.VisitedLocalities.Contains(garage)
                )
            {
                _cutscene = "cs10";
            }
            else
            {
                _cutscene = "cs9";
            }

            base.OnUseObject(message);
        }

    }
}
