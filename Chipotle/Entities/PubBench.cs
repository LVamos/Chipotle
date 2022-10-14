using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Represents a table in the pub (výčep h1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class PubBench : Item
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public PubBench(Name name, Rectangle area, bool decorative, bool pickable) : base(name, area, "hospodská lavice", decorative, pickable)
        { }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObjects(UseObjects message)
        {
            Character tuttle = World.GetCharacter("tuttle");

            if (
                !World.GetLocality("ulice h1").IsItHere(tuttle)
                && !SameLocality(tuttle)
                )
                _cutscene = "cs24";
            else
                _cutscene = "cs25";

            base.OnUseObjects(message);
        }
    }
}