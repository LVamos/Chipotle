using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using OpenTK;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Entities
{
    /// <summary>
    /// Controls movement of an NPC.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(ChipotlePhysicsComponent))]
    [ProtoInclude(101, typeof(TuttlePhysicsComponent))]
    public class PhysicsComponent : CharacterComponent
    {
        /// <summary>
        /// Checks if there's an character or item standing before the character and returns it.
        /// </summary>
        /// <returns>The character or item standing before the this character or null</returns>
        protected GameObject SomethingBefore()
        {
            GameObject i = ItemBefore();
            return i ?? CharacterBefore();
        }

        /// <summary>
        /// Checks if there's an character standing before the character and returns it.
        /// </summary>
        /// <returns>The character standing before the character or null</returns>
        protected Character CharacterBefore() => World.GetCharacter(GetNextTile(1).position);

        /// <summary>
        /// Checks if there's an item standing before the character and returns it.
        /// </summary>
        /// <returns>The item standing before the character or null</returns>
        protected Item ItemBefore() => World.GetObject(GetNextTile(1).position);

        /// <summary>
        /// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
        /// </summary>
        /// <param name="step">The distance between the NPC and the required tile</param>
        /// <returns>A reference to an tile that lays in the specified distance and direction</returns>
        /// <see cref="PhysicsComponent.Orientation"/>
        protected virtual Vector2 GetNextTile()
            => _path.Dequeue();

        /// <summary>
        /// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
        /// </summary>
        /// <param name="step">The distance between the NPC and the required tile</param>
        /// <returns>A reference to an tile that lays in the specified distance and direction</returns>
        /// <see cref="PhysicsComponent.Orientation"/>
        protected (Vector2 position, Tile tile) GetNextTile(int step)
            => GetNextTile(Orientation, step);

        /// <summary>
        /// Returns a tile at the specified distance and direction.
        /// </summary>
        /// <param name="direction">The direction of the tile to be found</param>
        /// <param name="step">The distance between the NPC and the tile to be found</param>
        /// <returns>A reference to an tile that lays in the specified distance and direction</returns>
        protected (Vector2 position, Tile tile) GetNextTile(Orientation2D direction, int step)
        {
            Rectangle target = new Rectangle(_area);
            target.Move(direction, step);
            Vector2 result = target.Center;
            result = new Vector2((float)Math.Round(result.X), (float)Math.Round(result.Y));
            return (result, World.Map[result]);
        }

        /// <summary>
        /// Stores the current shortest track towards the Detective Chipotle NPC.
        /// </summary>
        protected Queue<Vector2> _path;

        /// <summary>
        /// Reference to a path finder instance
        /// </summary>
        [ProtoIgnore]
        protected PathFinder _finder = new PathFinder();

        /// <summary>
        /// Solves a situation when the NPC encounters an obstacle.
        /// </summary>
        /// <param name="obstacle">Nearest point of the obstacle</param>
        protected virtual bool SolveObstacle(Vector2 obstacle)
            => true;

        /// <summary>
        /// Performs one step in direction specified in the _startWalkMessage field.
        /// </summary>
        protected virtual void MakeStep()
        {
            _speed = GetSpeed();
            _walkTimer = 0;
        }

        /// <summary>
        /// Specifies the length of one step in milliseconds.
        /// </summary>
        [ProtoIgnore]
        protected int _walkTimer;

        /// <summary>
        /// Performs walk on a preplanned route.
        /// </summary>
        protected virtual void PerformWalk()
        {
            if (_walkTimer < _speed)
                _walkTimer += World.DeltaTime;
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            PerformWalk();
        }

        /// <summary>
        /// The goal that Tuttle is just going to.
        /// </summary>
        protected Vector2 _goal;

        /// <summary>
        /// Returns reference to the tile the NPC currently stands on.
        /// </summary>
        [ProtoIgnore]
        protected (Vector2 position, Tile tile) CurrentTile
            => (_area.Center, World.Map[_area.Center]);

        protected float GetDistanceCoefficient(float distance)
        {
            if (distance < 5)
                return 2;

            if (distance < 10)
                return 1;

            if (distance < 30)
                return .5f;

            return .2f;
        }

        /// <summary>
        /// Computes length of the next step of the NPC.
        /// </summary>
        protected virtual int GetSpeed()
            => (int)(GetTerrainSpeed() * GetDistanceCoefficient(_path.Count));

        /// <summary>
        /// Returns speed of the terrain on which the NPC stands.
        /// </summary>
        /// <returns></returns>
        protected int GetTerrainSpeed()
            => _speeds[CurrentTile.tile.Terrain];

        /// <summary>
        /// Contains walk speed settings for particullar terrain types.
        /// </summary>
        protected readonly Dictionary<TerrainType, int> _speeds = new Dictionary<TerrainType, int>()
        {
            [TerrainType.Grass] = 600,
            [TerrainType.Linoleum] = 540,
            [TerrainType.Carpet] = 570,
            [TerrainType.Gravel] = 523,
            [TerrainType.Asphalt] = 440,
            [TerrainType.Cobblestones] = 510,
            [TerrainType.Tiles] = 400,
            [TerrainType.Wood] = 570,
            [TerrainType.Mud] = 940,
            [TerrainType.Puddle] = 650,
            [TerrainType.Concrete] = 468,
            [TerrainType.Clay] = 460,
            [TerrainType.Bush] = 970
        };

        /// <summary>
        /// Defines start position of the NPC.
        /// </summary>
        public Vector2? StartPosition { get; protected set; }

        /// <summary>
        /// Coordinates of the area currently occupied by the NPC
        /// </summary>
        protected Rectangle _area;

        /// <summary>
        /// Stores reference to a locality in which the NPC is currently located.
        /// </summary>
        public Locality Locality
        {
            get
            {
                return _area == null ? null : Owner.Locality;
            }
        }

        /// <summary>
        /// Current orientation of the NPC
        /// </summary>
        protected Orientation2D _orientation;

        /// <summary>
        /// Specifies the length of one step.
        /// </summary>
        [ProtoIgnore]
        protected int _speed;

        /// <summary>
        /// Current orientation of the NPC
        /// </summary>
        public Orientation2D Orientation => new Orientation2D(_orientation);

        /// <summary>
        /// Handles the GameReloaded message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected virtual void OnGameReloaded(GameReloaded message)
        {
            InnerMessage(new OrientationChanged(this, _orientation, _orientation, TurnType.None, true));
            InnerMessage(new PositionChanged(this, _area, _area, Locality, Locality, ObstacleType.None, true));
        }

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case GameReloaded gr: OnGameReloaded(gr); break;
                case SetPosition sp: OnSetPosition(sp); break;
                default: base.HandleMessage(message); break;
            }
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
        /// <param name="target">Coordinates of the target position</param>
        /// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
        protected virtual void Move(Vector2 target, bool silently = false)
            => Move(new Rectangle(target), silently);

        /// <summary>
        /// Immediately changes position of the NPC.
        /// </summary>
        /// <param name="target">coordinates of the target position</param>
        /// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
        protected void Move(Rectangle target, bool silently = false)
        {
            Locality sourceLocality = Locality;
            Locality targetLocality = target.GetLocalities().First();
            Rectangle sourcePosition = _area == null ? null : new Rectangle(_area);

            _area = new Rectangle(target);

            // If it isn't the player detect obstacles between him and this NPC.
            Character player = World.Player;
            ObstacleType obstacle = Owner != player ? World.DetectAcousticObstacles(_area) : ObstacleType.None;

            // Announce changes
            PositionChanged changed = new PositionChanged(this, sourcePosition, target, sourceLocality, targetLocality, obstacle, silently);
            InnerMessage(changed);

            if (targetLocality != sourceLocality)
                InnerMessage(new LocalityChanged(this, sourceLocality, targetLocality));
        }

        /// <summary>
        /// Processes the SetPosition message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnSetPosition(SetPosition message)
        => Move(message.Target, message.Silently);
    }
}