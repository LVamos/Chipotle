using System;
using System.Collections.Generic;

using Game.Entities;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a door in the hall of the Vanilla crunch company (hala v1) locality.
    /// </summary>
    [Serializable]
    public class HallDoor : Door
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the area the door occupies</param>
        /// <param name="localities">The localities connected by the door</param>
        public HallDoor(Name name, Plane area, IEnumerable<Locality> localities) : base(name, PassageState.Closed, area, localities)
        {
        }

        /// <summary>
        /// Opens the door if possible.
        /// </summary>
        protected override void Open(Vector2 position, object sender)
        {
            if (!World.GetObject("lavička w1").Used)
                return;

            base.Open(position, sender);
        }
    }
}