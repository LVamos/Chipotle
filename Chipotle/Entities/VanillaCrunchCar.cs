using Game.Messaging.Commands;
using Game.Terrain;

using Luky;


namespace Game.Entities
{
    public class VanillaCrunchCar : DumpObject
    {
        public VanillaCrunchCar(Name name, Plane area) : base(name, area, "auto Vanilla crunch") { }

        private KeyHanger KeyHanger => World.GetObject("věšák v1") as KeyHanger;


        protected override void OnUseObject(UseObject message)
        {
            _sounds.action = KeyHanger.KeysHanging ? "snd14" : "snd4";
            base.OnUseObject(message);
        }
    }
}
