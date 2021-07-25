using Game.Entities;

using Luky;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a passage between two localities e.g. a door.
    /// </summary>
    public class Passage : MapElement
    {
        protected override void Disappear()
        {
            _localities.ForEach(l => l.Unregister(this));
            Area.GetTiles().Foreach(t => t.UnregisterPassage());
        }





        public readonly IReadOnlyList<Locality> Localities;


        public readonly Locality ContainingLocality;








        private List<Locality> _localities;


        /// <summary>
        /// Returns another side of this passage.
        /// </summary>
        /// <param name="comparedLocality">source locality</param>
        /// <returns>remaining Locality</returns>
        public Locality AnotherLocality(Locality comparedLocality)
                    => _localities.First(l => l != comparedLocality);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isDoor">Is it a door?</param>
        /// <param name="closed">Is the passage closed from the beginning?</param>
        /// <param name="area">Area occupied with the passage</param>
        /// <param name="localities">Two localities connected with the passage</param>
        public Passage(Name name, Plane area, IEnumerable<Locality> localities) : base(name, area)
        {

            // Check if passage isn't on map edge and if it occupies just one row.
            Assert(area.Height == 1 || area.Height == 2 || area.Width == 1 || area.Width == 2, "Passage must consist of two rows or two points.");
            var coordinates = area.GetPoints().First();

            Assert(localities != null && localities?.Count() == 2 && localities.First() != null && localities.Last() != null && localities.First() != localities.Last(), "Two different localities required");
            _localities = localities.ToList<Locality>();
            Localities = _localities.AsReadOnly();

            // Validate passage location
            Assert(area.GetIntersectingObjects().IsNullOrEmpty() && area.GetIntersectingPassages().IsNullOrEmpty(), "No objects or nested passages allowed");

            Appear();
        }

        protected override void Appear()
        {
            Area.GetTiles().Foreach(t => t.Register(this));
            _localities.Foreach(l => l.Register(this));
        }

        public  static Passage CreatePassage(Name name, Plane area, IEnumerable<Locality> localities, bool isDoor, bool closed)
        {
switch(name.Indexed)
            {
                case "d hala w1": return CreateHallDoor(name, area, localities);
                default: return isDoor ? new Door(name, closed, area, localities) : new Passage(name, area, localities);
            }
        }

        public static HallDoor CreateHallDoor(Name name, Plane area, IEnumerable<Locality> localities)
=> new HallDoor(name, area, localities);
    }
}
