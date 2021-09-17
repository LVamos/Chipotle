using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Represents the bin object in the Walsch's pool (bazén w1) locality.
    /// </summary>
    public class PoolsideBin : DumpObject
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public PoolsideBin(Name name, Plane area) : base(name, area, "popelnice u bazénu", null, null, null, "cs3", true)
        { }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObject(UseObject message)
        {
            base.OnUseObject(message);

            if (UsedOnce)
            {
                Move(new Plane(new Vector2(911, 1042)));
            }
        }
    }
}