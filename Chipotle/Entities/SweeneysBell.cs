using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Represents the Sweeney's bell (zvonek p1) in the Easterby street (ulice p1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class SweeneysBell : Item
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public SweeneysBell(Name name, Rectangle area, bool decorative, bool pickable) : base(name, area, "Sweeneyho zvonek", decorative, pickable) { }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObjects(UseObjects message)
        {
            if (!Used)
                _cutscene = "cs23";
            else _sounds.action = "snd25";

            base.OnUseObjects(message);
        }
    }
}