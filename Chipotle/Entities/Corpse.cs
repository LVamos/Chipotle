using System;

using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Represents the tělo w1 object in the bazén w1 locality.
    /// </summary>
    /// <remarks>
    /// The object is destroyed when the Detective's car object moves out of the příjezdová cesta w1 locality.
    /// </remarks>
    [Serializable]
    public class Corpse : DumpObject
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name for the object</param>
        /// <param name="area">The coordinates of the area that the object occupies</param>
        public Corpse(Name name, Plane area, bool decorative) : base(name, area, "mrtvola", decorative, null, null, null, "cs5", true) { }

        /// <summary>
        /// Returns a reference to the Chipotle's car object.
        /// </summary>
        private ChipotlesCar Car
            => World.GetObject("detektivovo auto") as ChipotlesCar;

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (Car.Moved)
                Destroy();
        }
    }
}