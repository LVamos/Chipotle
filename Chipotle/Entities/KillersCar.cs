using Game.Messaging.Commands;
using Game.Terrain;

using Luky;



namespace Game.Entities
{
    public class KillersCar : DumpObject
    {
        public KillersCar(Name name, Plane area) : base(name, area, "vražedné auto v1") { }

        private bool KeysOnHanger
            => (World.GetObject("věšák v1") as KeyHanger).KeysHanging;

        private bool ChipotleHadIcecream
            => (World.GetObject("automat v1") as IcecreamMachine).Used;

        private ChipotlesCar ChipotlesCar => World.GetObject("detektivovo auto") as ChipotlesCar;

        protected override void OnUseObject(UseObject message)
        {
            if (KeysOnHanger)
                _sounds.action = "snd14";
            else if (ChipotleHadIcecream)
                _cutscene = "cs7";
            else
            {
                _cutscene = "cs8";
                ChipotlesCar.ReceiveMessage(new MoveChipotlesCar(this, World.GetLocality("ullice h1")));
            }

            base.OnUseObject(message);
        }

    }
}
