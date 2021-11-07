using System;
using System.Linq;

using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Represents a bench object in the zahrada c1 locality.
    /// </summary>
    [Serializable]
    public class CarsonsBench : DumpObject
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name for the object</param>
        /// <param name="area">The coordinates of the area that the object occupies</param>
        public CarsonsBench(Name name, Plane area, bool decorative) : base(name, area, "lavice u Carsona", decorative, null, null, null, "cs32", true)
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