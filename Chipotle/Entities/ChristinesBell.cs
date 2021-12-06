using System;

using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Represents the Christine's bell (zvonek p1) in the Belvedere street (ulice p1) locality.
    /// </summary>
    [Serializable]
    public class ChristinesBell : DumpObject
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public ChristinesBell(Name name, Plane area, bool decorative) : base(name, area, "Christinin zvonek", decorative) { }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObject(UseObject message)
        {
            if (!Used)
                _cutscene = "cs21";
            else _sounds.action = "snd25";

            base.OnUseObject(message);
        }
    }
}