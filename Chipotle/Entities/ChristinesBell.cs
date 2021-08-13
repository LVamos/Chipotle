using Game.Messaging.Commands;
using Game.Terrain;

using Luky;


namespace Game.Entities
{
    public class ChristinesBell : DumpObject
    {
        public ChristinesBell(Name name, Plane area) : base(name, area, "Christinin zvonek") { }

        protected override void OnUseObject(UseObject message)
        {
            if (!Used)
                _cutscene = "cs21";
            else _sounds.action = "snd25";

            base.OnUseObject(message);
        }
    }
}
