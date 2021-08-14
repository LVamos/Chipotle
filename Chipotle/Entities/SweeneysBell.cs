using Game.Messaging.Commands;
using Game.Terrain;

using Luky;


namespace Game.Entities
{
    public class SweeneysBell : DumpObject
    {
        public SweeneysBell(Name name, Plane area) : base(name, area, "Sweeneyho zvonek") { }

        protected override void OnUseObject(UseObject message)
        {
            if (!Used)
                _cutscene = "cs23";
            else _sounds.action = "snd25";

            base.OnUseObject(message);
        }


    }
}
