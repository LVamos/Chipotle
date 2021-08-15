using System;
using System.Collections.Generic;

using Game.Messaging;
using Game.Messaging.Events;

using Luky;


namespace Game.Terrain
{
    public class VanillaCrunchGarageDoor : Door
    {
        public VanillaCrunchGarageDoor(Name name, Plane area, IEnumerable<Locality> localities) : base(name, true, area, localities, false) { }

        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>()
                {
                    [typeof(LocalityEntered)] = (message) => OnLocalityEntered((LocalityEntered)message)
                });
        }

        private void OnLocalityEntered(LocalityEntered message)
            => Closed = false;
    }
}
