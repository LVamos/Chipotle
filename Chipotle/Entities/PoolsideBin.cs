using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using OpenTK;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Represents the bin object in the Walsch's pool (bazén w1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class PoolsideBin : Item
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public PoolsideBin(Name name, Rectangle area, bool decorative, bool pickable) : base(name, area, "popelnice u bazénu", decorative, pickable, null, null, null, "cs3", true)
        { }

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObjects(UseObjects message)
        {
            base.OnUseObjects(message);

            if (UsedOnce)
            {
                Move(new Rectangle(new Vector2(911, 1042)));
            }
        }
    }
}