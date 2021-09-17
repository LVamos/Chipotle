using System.Collections.Generic;
using System.Linq;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a passage between two localities.
    /// </summary>
    public class Passage : MapElement
    {
        /// <summary>
        /// Localities connected by the passage
        /// </summary>
        public readonly IReadOnlyList<Locality> Localities;

        /// <summary>
        /// Localities connected by the passage
        /// </summary>
        private List<Locality> _localities;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the passage</param>
        /// <param name="area">Coordinates of the are occupied by the passage</param>
        /// <param name="localities">Localities connected by the passage</param>
        public Passage(Name name, Plane area, IEnumerable<Locality> localities) : base(name, area)
        {
            // Check if passage isn't on map edge and if it occupies just one row.
            Assert(area.Height == 1 || area.Height == 2 || area.Width == 1 || area.Width == 2, "Passage must consist of two rows or two points.");
            Vector2 coordinates = area.GetPoints().First();

            Assert(localities != null && localities?.Count() == 2 && localities.First() != null && localities.Last() != null && localities.First() != localities.Last(), "Two different localities required");
            _localities = localities.ToList<Locality>();
            Localities = _localities.AsReadOnly();

            // Validate passage location
            Assert(area.GetIntersectingObjects().IsNullOrEmpty() && area.GetIntersectingPassages().IsNullOrEmpty(), "No objects or nested passages allowed");

            Appear();
        }

        /// <summary>
        /// Creates new instance of the door in the hall of the Vanilla crunch company (hala v1) locality.
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the are occupied by the door</param>
        /// <param name="localities">Localities connected by the door</param>
        /// <returns>A new instance of the door</returns>
        public static HallDoor CreateHallDoor(Name name, Plane area, IEnumerable<Locality> localities)
=> new HallDoor(name, area, localities);


        /// <summary>
        /// Creates new instance of the door between the hall of the Vanilla crunch company (hala v1) locality and the office of the Paolo Mariotti NPC's office (kancelář v1) locality.
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the are occupied by the door</param>
        /// <param name="localities">Localities connected by the door</param>
        /// <returns>A new instance of the door</returns>
        public static MariottisDoor CreateMariottisDoor(Name name, Plane area, IEnumerable<Locality> localities)
=> new MariottisDoor(name, area, localities);

        /// <summary>
        /// Creates new instance of a passage according to the given parameters.
        /// </summary>
        /// <param name="name">Inner name of the passage</param>
        /// <param name="area">Coordinates of the are occupied by the passage</param>
        /// <param name="localities">Localities connectedd by the passage</param>
        /// <param name="isDoor">Specifies if the passage is a door.</param>
        /// <param name="closed">Specifies if the passage is closed.</param>
        /// <param name="openable">Specifies if it can be opened by an NPC.</param>
        /// <returns>A new instance of the passage</returns>
        public static Passage CreatePassage(Name name, Plane area, IEnumerable<Locality> localities, bool isDoor, bool closed, bool openable)
        {
            switch (name.Indexed)
            {
                case "dcgv1": return CreateVanillaCrunchGarageDoor(name, area, localities);
                case "dhkv1": return CreateMariottisDoor(name, area, localities);
                case "d hala w1": return CreateHallDoor(name, area, localities);
                default: return isDoor ? new Door(name, closed, area, localities, openable) : new Passage(name, area, localities);
            }
        }

        /// <summary>
        /// Returns another side of this passage.
        /// </summary>
        /// <param name="comparedLocality">The locality to be compared</param>
        /// <returns>The other side of the passage than the specified one</returns>
        public Locality AnotherLocality(Locality comparedLocality)
                    => _localities.First(l => l != comparedLocality);

        /// <summary>
        /// Displays the passage in the game world.
        /// </summary>
        protected override void Appear()
        {
            Area.GetTiles().Foreach(t => t.Register(this));
            _localities.Foreach(l => l.Register(this));
        }

        /// <summary>
        /// Erases the passage from the game world.
        /// </summary>
        protected override void Disappear()
        {
            _localities.ForEach(l => l.Unregister(this));
            Area.GetTiles().Foreach(t => t.UnregisterPassage());
        }

        /// <summary>
        /// Creates new instance of the garage door in the garage of the Vanilla crunch company (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the are occupied by the door</param>
        /// <param name="localities">Localities connected by the door</param>
        /// <returns>A new instance of the door</returns>
        private static Passage CreateVanillaCrunchGarageDoor(Name name, Plane area, IEnumerable<Locality> localities)
            => new VanillaCrunchGarageDoor(name, area, localities);
    }
}