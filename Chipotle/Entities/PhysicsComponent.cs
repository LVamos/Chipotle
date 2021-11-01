using System;
using System.Collections.Generic;

using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Controls movement of an NPC.
    /// </summary>
    [Serializable]
    public class PhysicsComponent : EntityComponent
    {
        /// <summary>
        /// Coordinates of the area currently occupied by the NPC
        /// </summary>
        protected Plane _area;

        /// <summary>
        /// Stores reference to a locality in which the NPC is currently located.
        /// </summary>
        protected Locality _locality;

        /// <summary>
        /// Current orientation of the NPC
        /// </summary>
        protected Orientation2D _orientation;

        /// <summary>
        /// Specifies the length of one step.
        /// </summary>
        protected int _walkSpeed;

        /// <summary>
        /// Current orientation of the NPC
        /// </summary>
        public Orientation2D Orientation => new Orientation2D(_orientation);

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
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

        /// <summary>
        /// Calculates an angle between the NPC and a specified point.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="b"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        protected double GetAngle(Vector2 point)
            => Luky.MathHelper.GetAngle(point, _area.Center, _orientation);

        /// <summary>
        /// Immediately changes position of the NPC.
        /// </summary>
        /// <param name="x">X coordinate of the target position</param>
        /// <param name="y">Y coordinate of the target position</param>
        /// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
        protected void SetPosition(float x, float y, bool silently = false)
            => SetPosition(new Vector2(x, y), silently);

        /// <summary>
        /// Immediately changes position of the NPC.
        /// </summary>
        /// <param name="coords">Coordinates of the target position</param>
        /// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
        protected void SetPosition(Vector2 coords, bool silently = false)
            => SetPosition(new Plane(coords), silently);

        /// <summary>
        /// Immediately changes position of the NPC.
        /// </summary>
        /// <param name="target">coordinates of the target position</param>
        /// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
        protected void SetPosition(Plane target, bool silently = false)
        {
            Tile targetTile = World.Map[target.Center];
            Locality sourceLocality = _area?.GetLocality();
            Locality targetLocality = target.GetLocality();
            Plane source = _area;

            _area = new Plane(target);
            _locality = _area.GetLocality();

            // Announce changes
            Owner.ReceiveMessage(new PositionChanged(this, source, target));

            if (!silently)
            {
                EntityMoved moved = new EntityMoved(Owner, target.Center);
                Owner.ReceiveMessage(new EntityMoved(this, target.Center));
                targetLocality.ReceiveMessage(moved);
            }

            if (targetLocality != sourceLocality)
            {
                sourceLocality?.ReceiveMessage(new LocalityLeft(Owner, Owner));
                targetLocality.ReceiveMessage(new LocalityEntered(Owner, Owner));
                Owner.ReceiveMessage(new LocalityChanged(this, sourceLocality, targetLocality));
            }
        }

        /// <summary>
        /// Processes the SetPosition message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSetPosition(SetPosition message)
            => SetPosition(message.Target, message.Silently);
    }
}