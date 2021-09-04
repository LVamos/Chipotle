using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;

namespace Game.Entities
{
    public abstract class PhysicsComponent : EntityComponent
    {


        protected int _walkSpeed;

        protected void Appear(Plane target) => target.GetTiles().Foreach(t => t.Register(Owner));

        protected void DisAppear()
            => _area.GetTiles()
            .Foreach(t => t.UnregisterObject());

        protected void SetPosition(float x, float y, bool silently=false)
            => SetPosition(new Vector2(x, y), silently);

        protected void SetPosition(Vector2 coords, bool silently=false)
            => SetPosition(new Plane(coords), silently);

        protected void SetPosition(Plane target, bool silently=false)
        {
            Tile targetTile = World.Map[target.Center];
            Locality sourceLocality = _area?.GetLocality();
            Locality targetLocality = target.GetLocality();


            if (_area != null)
                DisAppear();

            Appear(target);
            _area = new Plane(target);

            // Announce changes
            Owner.ReceiveMessage(new PositionChanged(this, _area));

            if(!silently)
            {
            EntityMoved moved = new EntityMoved(Owner, targetTile);
            Owner.ReceiveMessage(new EntityMoved(this, targetTile));
            targetLocality.ReceiveMessage(moved);
            }

            if (targetLocality != sourceLocality)
            {
                sourceLocality?.ReceiveMessage(new LocalityLeft(Owner, Owner));
                targetLocality.ReceiveMessage(new LocalityEntered(Owner, Owner));
                Owner.ReceiveMessage(new LocalityChanged(this, sourceLocality, targetLocality));
            }
        }

        public override void Start()
        {
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<Messaging.GameMessage>>
                {
                    [typeof(SetPosition)] = (m) => OnSetPosition((SetPosition)m),
                }
                );
        }

        private void OnSetPosition(SetPosition m)
            => SetPosition(m.Target, m.Silently);

        protected Orientation2D _orientation;
        public Orientation2D Orientation => new Orientation2D(_orientation);

        protected Plane _area;



    }
}
