using Game.Messaging;
using Game.Messaging.Commands;

using Luky;

using System;
using System.Collections.Generic;

namespace Game.Terrain
{
    public abstract class MapElement : MessagingObject
    {


        protected virtual void Destroy()
        {
            _messagingEnabled = false;
            Disappear();
        }

        protected virtual void Appear() { }
        protected virtual void Disappear()
        {
        }



        public MapElement(Name name, Plane area) : base()
        {
            Name = name ?? throw new ArgumentException(nameof(name));
            _area = area;// ?? throw new ArgumentException(nameof(area));
        }

        public readonly Name Name;

        protected Plane _area;

        public Plane Area
            => new Plane(_area);

        public override string ToString()
=> Name.Friendly;
        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>
                {
                    [typeof(Destroy)] = (m) => OnDestroy((Destroy)m)
                }
                );
        }

        protected virtual void OnDestroy(Destroy m)
            => Destroy();
    }
}
