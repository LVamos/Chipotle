using System;
using System.Collections.Generic;

using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    internal class TuttlePhysicsComponent : PhysicsComponent
    {

        private Random _random = new Random();
        private PathFinder _finder = new PathFinder();

        private Entity _player;
        private bool _approachToPlayer;
        private bool _walk;
        private float _stepInterval;
        private int _minWalkSpeed = 400;
        private int _maxWalkSpeed = 900;
        private const int _maxDistanceFromPlayer = 10;
        private const int _minDistanceFromPlayer = 2;
        private int _desiredDistanceFromPlayer;
        private Queue<Vector2> _path;
        private bool _followPlayer;

        public override void Start()
        {
            _orientation = new Orientation2D(0, 1);
            _player = World.Player;

            RegisterMessages(new Dictionary<Type, Action<Messaging.GameMessage>>
            {
                [typeof(Hide)] = (message) => OnHide((Hide)message),
                [typeof(GotoPoint)] = (m) => OnGotoPoint((GotoPoint)m),
                [typeof(StartFollowing)] = (m) => OnStartFollowing((StartFollowing)m),
                [typeof(StopFollowing)] = (m) => OnStopFollowing((StopFollowing)m),
                [typeof(LocalityLeft)] = (message) => OnLocalityLeft((LocalityLeft)message),
                [typeof(EntityMoved)] = (message) => OnEntityMoved((EntityMoved)message)
            }
                );

            base.Start();
        }


        private bool _hidden;

        private void OnHide(Hide message)
        {
            StopFollowing();
            DisAppear();
            _hidden = true;
        }


        private void OnGotoPoint(GotoPoint m)
        {
            _path = m.Path;
            _walkSpeed = m.StepLength;
            _walk = true;
        }

        private void OnStartFollowing(StartFollowing m)
            => _followPlayer = true;

        private void OnStopFollowing(StopFollowing m)
            => StopFollowing();

        private void OnLocalityLeft(LocalityLeft message)
        {
            if (message.Sender != _player)
            {
                return;
            }

            StopApproachingToPlayer();
            GoToPlayer();
        }

        private void StopFollowing()
        {
            _followPlayer = false;
            StopWalk();
        }
        private void OnEntityMoved(EntityMoved message)
        {
            if (message.Sender != _player)
            {
                return;
            }

            if (_followPlayer)
                CheckDistanceFromPlayer();
        }

        private void CheckDistanceFromPlayer()
        {
            int distance = GetDistanceFromPlayer();
            if (distance > _maxDistanceFromPlayer && !_approachToPlayer)
            {
                GoToPlayer();
            }
            else if (_approachToPlayer && (distance <= _desiredDistanceFromPlayer || _path.IsNullOrEmpty()))
            {
                StopApproachingToPlayer();
            }
        }

        private void StopApproachingToPlayer()
        {
            _walk = _approachToPlayer = false;
            _path = null;
        }

        private int GetDistanceFromPlayer()
=> GetDistance(_area.Center, _player.Area.Center);

        private int GetDistance(Vector2 a, Vector2 b)
=> (int)(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));

        private void GoToPlayer()
        {
            _desiredDistanceFromPlayer = _random.Next(_minDistanceFromPlayer, _maxDistanceFromPlayer);
            Vector2? target = FindPlaceNearPlayer();

            if (!target.HasValue)
            {
                return;
            }

            _path = _finder.FindPath(_area.Center, (Vector2)target);

            if (_path == null)
                return;

            _approachToPlayer = _walk = true;
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

        private Vector2? FindPlaceNearPlayer()
=> _player.Area.FindNearestWalkableTile(_maxDistanceFromPlayer);

        public override void Update()
        {
            base.Update();

            if (_walk)
            {
                PerformWalk();
            }
        }

        private void PerformWalk()
        {
            if (_approachToPlayer)
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

            if (_path.IsNullOrEmpty())
                StopWalk();
        }

        private void StopWalk()
        {
            _walk = false;
            _path = null;
            _stepInterval = 0;
        }

        private void Step()
        {
            // Get target tile
            Plane target = new Plane(_path.Dequeue());
            Tile targetTile = World.Map[target.Center];

            if (!targetTile.Walkable && (targetTile.IsOccupied && targetTile.Object != Owner))
            {
                throw new InvalidOperationException("Lost");
                StopApproachingToPlayer();
                return;
            }

            // The road is clear! Move!
            SetPosition(target);
        }
    }
}
