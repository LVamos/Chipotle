using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using ProtoBuf;

using System.Linq;

namespace Game.Entities
{
    /// <summary>
    /// Represents a bench object in the zahrada c1 locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class CarsonsBench : Item
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name for the object</param>
        /// <param name="area">The coordinates of the area that the object occupies</param>
        public CarsonsBench(Name name, Rectangle area, bool decorative, bool pickable) : base(name, area, "lavice u Carsona", decorative, pickable, null, null, null, "cs32", true)
        { }

        /// <summary>
        /// Returns a reference to the Chipotle's car object.
        /// </summary>
        private ChipotlesCar Car
            => World.GetObject("detektivovo auto") as ChipotlesCar;

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObjects(UseObjects message)
        {
            if (
                !World.GetObjectsByType("lavice u Carsona")
                .Any(o => o.Used)
                )
            {
                base.OnUseObjects(message);
                Car.TakeMessage(new UnblockLocality(this, World.GetLocality("ulice v1")));
            }
        }
    }
}