using Luky;

using OpenTK;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Terrain
{
    /// <summary>
    /// Represents the door between the hall of the Vanilla crunch company and (hala v1) locality
    /// and the office of the Paolo Mariotti office (kancelář v1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class MariottisDoor : Door
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the area the door occupies</param>
        /// <param name="localities">The localities connected by the door</param>
        public MariottisDoor(Name name, Rectangle area, IEnumerable<string> localities) : base(name, PassageState.Closed, area, localities) { }

        /// <summary>
        /// Opens the door if possible.
        /// </summary>
        /// <param name="coords">
        /// The coordinates of the place on the door that an NPC is pushing on
        /// </param>
        protected override void Open(object sender, Vector2 point)
            => World.PlayCutscene(this, "cs11");
    }
}