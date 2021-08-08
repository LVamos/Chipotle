using Game.Messaging.Commands;
using Game.Terrain;

using Luky;


namespace Game.Entities
{
    public class PubTable : DumpObject
    {
        public PubTable(Name name, Plane area) : base(name, area, "hospodský stůl")
        { }

        protected override void OnUseObject(UseObject message)
        {
            Entity tuttle = World.GetEntity("tuttle");

            if (
                !World.GetLocality("ulice h1").IsItHere(tuttle)
                && !Locality.IsItHere(tuttle)
                )
            {
                _cutscene = "cs24";
            }
            else
            {
                _cutscene = "cs25";
            }

            base.OnUseObject(message);
        }

    }
}
