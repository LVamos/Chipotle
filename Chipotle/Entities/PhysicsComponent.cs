using System;
using System.Collections.Generic;
using System.Linq;

using DavyKager;

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
        /// Defines start position of the NPC.
        /// </summary>
        public Vector2? StartPosition { get; protected set; }

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
            => World.GetAngle(point, _area.Center, _orientation);

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
        /// <param name="targetPosition">coordinates of the target position</param>
        /// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
        protected void SetPosition(Plane targetPosition, bool silently = false)
        {
            Locality sourceLocality = _area?.GetLocality();
            Locality targetLocality = targetPosition.GetLocality();
            Plane sourcePosition = _area;

            _area = new Plane(targetPosition);
            _locality = _area.GetLocality();

            // If it isn't the player detect obstacles between him and this NPC.
            Entity player = World.Player;
            ObstacleType obstacle = Owner != player ? World.DetectAcousticObstacles(_area) : ObstacleType.None;

            // Announce changes
            PositionChanged changed = new PositionChanged(this, sourcePosition, targetPosition, sourceLocality, targetLocality, obstacle, silently);
            Owner.ReceiveMessage(changed);

            if (targetLocality != sourceLocality)
                Owner.ReceiveMessage(new LocalityChanged(this, sourceLocality, targetLocality));
            }

            /// <summary>
            /// Processes the SetPosition message.
            /// </summary>
            /// <param name="message">The message to be processed</param>
            private void OnSetPosition(SetPosition message)
            => SetPosition(message.Target, message.Silently);
    }
}