using Luky;

using OpenTK;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a door in the hall of the Vanilla crunch company (hala v1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class HallDoor : Door
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the area the door occupies</param>
        /// <param name="localities">The localities connected by the door</param>
        public HallDoor(Name name, Rectangle area, IEnumerable<string> localities) : base(name, PassageState.Closed, area, localities)
        {
        }

        /// <summary>
        /// Opens the door if possible.
        /// </summary>
        protected override void Open(object sender, Vector2 point)
        {
            if (!World.GetObject("lavička w1").Used)
                return;

            base.Open(sender, point);
        }
    }
}