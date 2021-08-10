using Game.Messaging.Commands;
using Game.Terrain;

using Luky;


namespace Game.Entities
{
    public class KeyHanger : DumpObject
    {
        public KeyHanger(Name name, Plane area) : base(name, area, "věšák na klíče") { }

        protected override void OnUseObject(UseObject message)
        {
            _sounds.action = KeysHanging ? "snd6" : "snd5";
            KeysHanging = !KeysHanging;
            base.OnUseObject(message);
        }


        public bool KeysHanging { get; private set; } = true;
    }
}
