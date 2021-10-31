using System;
using System.Linq;

using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Represents the icecream machine object (automat v1) )in the hall of the Vanilla crunch
    /// company (hala v1) locality.
    /// </summary>
    [Serializable]
    public class IcecreamMachine : DumpObject
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public IcecreamMachine(Name name, Plane area) : base(name, area, "automat na zmrzlinu", null, null, "VendingMachineLoop")
        { }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObject(UseObject message)
        {
            Entity tuttle = World.GetEntity("tuttle");
            Locality garage = World.GetLocality("garáž v1");
            if (
                Locality.IsItHere(tuttle)
                && tuttle.VisitedLocalities.Contains(garage)
                && World.Player.VisitedLocalities.Contains(garage)
                )
                _cutscene = "cs10";
            else
                _cutscene = "cs9";

            base.OnUseObject(message);
        }
    }
}