using ProtoBuf;
using System;

using Game.Messaging.Commands;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Represents a table in the pub (výčep h1) locality.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class PubTable : DumpObject
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public PubTable(Name name, Plane area, bool decorative) : base(name, area, "hospodský stůl", decorative)
        { }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnUseObject(UseObject message)
        {
            Entity tuttle = World.GetEntity("tuttle");

            if (
                !World.GetLocality("ulice h1").IsItHere(tuttle)
                && !Locality.IsItHere(tuttle)
                )
                _cutscene = "cs24";
            else
                _cutscene = "cs25";

            base.OnUseObject(message);
        }
    }
}