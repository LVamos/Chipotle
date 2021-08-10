using Game.Terrain;
using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;

using Luky;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Game.Entities
{
    public class KillersCar: DumpObject
    {
        public KillersCar(Name name, Plane area) : base(name, area, "vražedné auto v1") { }

        private bool KeysOnHanger 
            => (World.GetObject("věšák v1") as KeyHanger).KeysHanging;

        private bool ChipotleHadIcecream
            => (World.GetObject("automat v1") as IcecreamMachine).Used;

        protected override void OnUseObject(UseObject message)
        {
            if (KeysOnHanger)
                _sounds.action = "snd14";
            else _cutscene = ChipotleHadIcecream ? "cs7" : "cs8";

            base.OnUseObject(message);
        }

    }
}
