
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
        public IcecreamMachine(Name name, Plane area) : base(name, area, "automat na zmrzlinu")
        { }

        protected override void OnUseObject(UseObject message)
        {
            base.OnUseObject(message);

            Entity tuttle = World.GetEntity("tuttle");
            Locality garage = World.GetLocality("garáž v1");
            if (
                Locality.IsItHere(tuttle)
                && tuttle.VisitedLocalities.Contains(garage)
                && World.Player.VisitedLocalities.Contains(garage)
                )
            {
                World.PlayCutscene(this, "cs10");
            }
            else
            {
                World.PlayCutscene(this, "cs9");
            }
        }

    }
}
