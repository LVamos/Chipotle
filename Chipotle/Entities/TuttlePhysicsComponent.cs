using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Entities
{
    internal class TuttlePhysicsComponent : PhysicsComponent
    {

        private Random _random = new Random();
        private PathFinder _finder = new PathFinder();

        private Entity _player;
        private int _pathIndex;
        private bool _approachingToPlayer;
        private bool _walking;
        private float _stepInterval;
        private int _minWalkSpeed = 400;
        private int _maxWalkSpeed = 900;
        private const int _maxDistanceFromPlayer = 10;
        private const int _minDistanceFromPlayer = 2;
        private int _desiredDistanceFromPlayer;
        private List<Vector2> _path;

        public override void Start()
        {
            // set initial position.
            SetPosition(new Plane(new Vector2(1030, 1030)));
            _orientation = new Orientation2D(0, 1);
            _player = World.Player;
            RegisterMessages(new Dictionary<Type, Action<Messaging.GameMessage>>
            {
                [typeof(LocalityLeft)] = (message) => OnLocalityLeft((LocalityLeft)message),
                [typeof(EntityMoved)] = (message) => OnEntityMoved((EntityMoved)message)
            }
                );

            _area.GetLocality().ReceiveMessage(new LocalityEntered(Owner, Owner));
            base.Start();
        }

        private void OnLocalityLeft(LocalityLeft message)
        {
            if (message.Sender != _player)
            {
                return;
            }

            StopApproachingToPlayer();
            GoToPlayer();
        }

        private void OnEntityMoved(EntityMoved message)
        {
            if (message.Sender != _player)
            {
                return;
            }

            CheckDistanceFromPlayer();
        }

        private void CheckDistanceFromPlayer()
        {
            int distance = GetDistanceFromPlayer();
            if (distance > _maxDistanceFromPlayer && !_approachingToPlayer)
            {
                GoToPlayer();
            }
            else if (_approachingToPlayer && (distance <= _desiredDistanceFromPlayer || _pathIndex <= 0))
            {
                StopApproachingToPlayer();
            }
        }

        private void StopApproachingToPlayer()
        {
            _walking = _approachingToPlayer = false;
            _path = null;
        }

        private int GetDistanceFromPlayer()
=> GetDistance(_area.Center, _player.Area.Center);

        private int GetDistance(Vector2 a, Vector2 b)
=> (int)(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));

        private void GoToPlayer()
        {
            _desiredDistanceFromPlayer = _random.Next(_minDistanceFromPlayer, _maxDistanceFromPlayer);
            (bool found, Vector2 target) target = FindPlaceNearPlayer();

            if (!target.found)
            {
                return;
            }

            _path = _finder.FindPath(_area.Center, target.target);

            if (_path == null)
            {
                return;
            }

            _pathIndex = _path.Count - 1;
            _approachingToPlayer = _walking = true;
            _walkSpeed = ComputeWalkSpeed(GetDistanceFromPlayer());
            _stepInterval = 0;
        }

        private int ComputeWalkSpeed(int distance)
        {
            if (distance < 5)
            {
                return 1200;
            }

            if (distance < 7)
            {
                return 1000;
            }

            if (distance < 10)
            {
                return 900;
            }

            if (distance < 15)
            {
                return 500;
            }

            if (distance < 20)
            {
                return 300;
            }

            return 150;
        }

        private (bool found, Vector2 point) FindPlaceNearPlayer()
        {
            Plane aroundPlayer = new Plane(_player.Area);
            for (int i = 0; i <= _maxDistanceFromPlayer; i++)
            {
                aroundPlayer.Extend();
                Tile walkable = aroundPlayer.GetPerimeterTiles().FirstOrDefault(t => t.Walkable);

                if (walkable != null)
                {
                    return (true, walkable.Position);
                }
            }
            return (false, default);
        }

        public override void Update()
        {
            base.Update();

            if (_walking)
            {
                PerformWalk();
            }
        }

        private void PerformWalk()
        {
            CheckDistanceFromPlayer();

            if (_stepInterval >= _walkSpeed)
            {
                _stepInterval = 0;
                Step();
            }
            else
            {
                _stepInterval += World.DeltaTime;
            }
        }


        private void Step()
        {
            // Get target tile
            Plane target = new Plane(_path[_pathIndex--]);
            Tile targetTile = World.Map[target.Center];

            if (!targetTile.Permeable || (targetTile.IsOccupied && targetTile.Object != Owner))
            {
                SayDelegate("Kam mám jít?");
                StopApproachingToPlayer();
                return;
            }

            // The road is clear! Move!
            Locality sourceLocality = _area.GetLocality();
            Locality targetLocality = World.Map[target.Center].Locality;
            EntityMoved moved = new EntityMoved(Owner, targetTile);
            SetPosition(target);
            Owner.ReceiveMessage(new EntityMoved(this, targetTile));
            targetLocality.ReceiveMessage(moved);

            if (targetLocality != sourceLocality)
            {
                sourceLocality.ReceiveMessage(new LocalityLeft(Owner, Owner));
                targetLocality.ReceiveMessage(new LocalityEntered(Owner, Owner));
            }

        }
    }
}
