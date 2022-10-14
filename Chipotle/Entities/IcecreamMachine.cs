using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using ProtoBuf;

using System;
using System.Linq;

namespace Game.Entities
{
    /// <summary>
    /// Represents the icecream machine object (automat v1) )in the hall of the Vanilla crunch
    /// company (hala v1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class IcecreamMachine : Item
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public IcecreamMachine(Name name, Rectangle area, bool decorative, bool pickable) : base(name, area, "automat na zmrzlinu", decorative, pickable, null, null, "VendingMachineLoop")
        { }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObjects(UseObjects message)
        {
            Character tuttle = World.GetCharacter("tuttle");
            Locality garage = World.GetLocality("garáž v1");
            if (
                SameLocality(tuttle)
                && tuttle.VisitedLocalities.Contains(garage)
                && World.Player.VisitedLocalities.Contains(garage)
                )
                _cutscene = "cs10";
            else
                _cutscene = "cs9";

            base.OnUseObjects(message);
        }
    }
}