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
        protected float GetDistanceCoefficient(float distance, float minSpeed = .5f, float maxSpeed = 2, float distanceInterval= 5, float minDistance = 10, float maxDistance = 80)
        {
            if (distance > maxDistance)
                return minSpeed;

            if (distance < minDistance)
                return maxSpeed;

            float speedInterval = (maxSpeed - minSpeed) / ((maxDistance - minDistance) / distanceInterval);
                return maxSpeed - (speedInterval * (distance / distanceInterval));
        }

        /// <summary>
        /// Computes length of the next step of the NPC.
        /// </summary>
        /// <param name="nextStep">Target coordinates of next step of the NPC</param>
        protected virtual int GetSpeed(Vector2 nextStep)
            => _speeds[World.Map[nextStep].Terrain];

        /// <summary>
        /// Computes length of the next step of the NPC.
        /// </summary>
        /// <param name="nextStep">Coordinates of the next location in the game world</param>
        /// <param name="goal">Coordinates of the goal of an ongoing movement of the NPC</param>
        protected virtual int GetSpeed(Vector2 nextStep, Vector2 goal)
            => (int) (GetSpeed(nextStep) * GetDistanceCoefficient(World.GetDistance(Owner.Area.Center, goal)));

        /// <summary>
        /// Contains walk speed settings for particullar terrain types.
        /// </summary>
        protected readonly Dictionary<TerrainType, int> _speeds = new Dictionary<TerrainType, int>()
        {
            [TerrainType.Grass] = 430,
            [TerrainType.Linoleum] = 300,
            [TerrainType.Carpet] = 340,
            [TerrainType.Gravel] = 360,
            [TerrainType.Asphalt] = 310,
            [TerrainType.Cobblestones] = 410,
            [TerrainType.Tiles] = 300,
            [TerrainType.Wood] = 330,
            [TerrainType.Mud] = 500,
            [TerrainType.Puddle] = 450,
            [TerrainType.Concrete] = 280,
            [TerrainType.Clay] = 406,
            [TerrainType.Bush] = 600
        };

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
        protected int _speed;

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
        protected void Move(float x, float y, bool silently = false)
            => Move(new Vector2(x, y), silently);

        /// <summary>
        /// Immediately changes position of the NPC.
        /// </summary>
        /// <param name="coords">Coordinates of the target position</param>
        /// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
        protected void Move(Vector2 coords, bool silently = false)
            => Move(new Plane(coords), silently);

        /// <summary>
        /// Immediately changes position of the NPC.
        /// </summary>
        /// <param name="targetPosition">coordinates of the target position</param>
        /// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
        protected void Move(Plane targetPosition, bool silently = false)
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
            => Move(message.Target, message.Silently);
    }
}