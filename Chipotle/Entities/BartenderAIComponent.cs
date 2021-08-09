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
    public class BartenderAIComponent : AIComponent
    {
        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
                    [typeof(LocalityEntered)] = (m) => OnLocalityEntered((LocalityEntered)m),
                    [typeof(EntityMoved)] = (m) => OnEntityMoved((EntityMoved)m),

                }
                );
            Owner.ReceiveMessage(new SetPosition(this, new Plane("1577, 1037")));
        
        }

        private void OnEntityMoved(EntityMoved message)
        {
            if (message.Sender != World.Player)
                return;

            Entity player = World.Player;
            Passage door = World.GetPassage("duvh1");
            bool tableUsed = World.GetObjectsByType("hospodský stůl").Any(o => o.Used);

            if (
                _sayGoodbyeToChipotle
                &&tableUsed
              && _area.GetLocality().IsItHere(player)
              && door.Area.GetDistanceFrom(player.Area.Center) == 1
                )
            {
                World.PlayCutscene(Owner, IsChipotleAlone() ? "cs28" : "cs29");
                _sayGoodbyeToChipotle = false;
            }
        }

        private bool _sayGoodbyeToChipotle;
        private bool _velcomeChipotle = true;
        private bool _wasCarNearBy;

        public override void Update()
        {
            base.Update();

            WatchChipotlesCar();
        }


        private bool IsChipotleAlone()
        {
            Locality street = World.GetLocality("ulice h1");
            Locality here = _area.GetLocality();
            Entity tuttle = World.GetEntity("tuttle");

            return !here.IsItHere(tuttle) && !street.IsItHere(tuttle);
        }

        private void WatchChipotlesCar()
        {
            bool isCarNearBy= IsChipotlesCarNearBy();
            if(isCarNearBy!=_wasCarNearBy)
            {

            _velcomeChipotle = isCarNearBy;
                _wasCarNearBy = isCarNearBy;
            }
        }

        private readonly Locality BonitaStreet = World.GetLocality("ulice h1");
        private ChipotlesCar ChipotlesCar
            => World.GetObject("detektivovo auto") as ChipotlesCar;

        private bool IsChipotlesCarNearBy()
            => BonitaStreet.IsItHere(ChipotlesCar);

        private void OnLocalityEntered(LocalityEntered message)
        {
            if (message.Sender !=    World.Player)
                return;

            if(_velcomeChipotle)
            {
                World.PlayCutscene(Owner, IsChipotleAlone() ? "cs30" : "cs31");
                _velcomeChipotle = false;
                _sayGoodbyeToChipotle = true;
            }
        }
    }
}
