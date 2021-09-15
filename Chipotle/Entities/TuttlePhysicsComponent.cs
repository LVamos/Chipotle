using System;
using System.Collections.Generic;

using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

namespace Game.Entities
{
    public class TuttlePhysicsComponent : PhysicsComponent
    {
        private const int _maxDistanceFromPlayer = 10;
        private const int _minDistanceFromPlayer = 2;
        private bool _approachToPlayer;
        private int _desiredDistanceFromPlayer;
        private PathFinder _finder = new PathFinder();
        private bool _followPlayer;
        private bool _hidden;
        private Queue<Vector2> _path;
        private Entity _player;
        private Random _random = new Random();
        private float _stepInterval;
        private bool _walk;

        public override void Start()
        {
            _orientation = new Orientation2D(0, 1);
            _player = World.Player;

            RegisterMessages(new Dictionary<Type, Action<Messaging.GameMessage>>
            {
                [typeof(Reveal)] = (message) => OnReveal((Reveal)message),
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

        public override void Update()
        {
            base.Update();

            if (_walk)
                PerformWalk();
        }

        private int ComputeWalkSpeed(int distance)
        {
            if (distance < 5)
                return 1200;

            if (distance < 7)
                return 1000;

            if (distance < 10)
                return 900;

            if (distance < 15)
                return 500;

            if (distance < 20)
                return 300;

            return 150;
        }

        private Vector2? FindPlaceNearPlayer()
=> _player.Area.FindNearestWalkableTile(_maxDistanceFromPlayer);

        private int GetDistance(Vector2 a, Vector2 b)
=> (int)(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));

        private int GetDistanceFromPlayer()
=> GetDistance(_area.Center, _player.Area.Center);

        private void GoToPlayer()
        {
            _desiredDistanceFromPlayer = _random.Next(_minDistanceFromPlayer, _maxDistanceFromPlayer);
            Vector2? target = FindPlaceNearPlayer();

            if (!target.HasValue)
                return;
            _path = _finder.FindPath(_area.Center, (Vector2)target);

            if (_path == null)
                return;

            _approachToPlayer = _walk = true;
            _walkSpeed = ComputeWalkSpeed(GetDistanceFromPlayer());
            _stepInterval = 0;
        }

        private void CheckDistanceFromPlayer()
        {
            int distance = GetDistanceFromPlayer();
            if (distance > _maxDistanceFromPlayer && !_approachToPlayer)
                GoToPlayer();
            else if (_approachToPlayer && (distance <= _desiredDistanceFromPlayer || _path.IsNullOrEmpty()))
                StopApproachingToPlayer();
        }

        private void OnEntityMoved(EntityMoved message)
        {
            if (message.Sender != _player)
                return;

            if (_followPlayer && !_approachToPlayer)
                CheckDistanceFromPlayer();
        }

        private void OnGotoPoint(GotoPoint m)
        {
            _path = m.Path;
            _walkSpeed = m.StepLength;
            _walk = true;
        }

        private void OnHide(Hide message)
        {
            StopFollowing();
            DisAppear();
            _hidden = true;
        }

        private void OnLocalityLeft(LocalityLeft message)
        {
            if (message.Sender != _player)
                return;

            StopApproachingToPlayer();
            GoToPlayer();
        }

        private void OnReveal(Reveal message)
        {
            _area = message.Location;
            Appear(message.Location);
            _hidden = false;
            StartFollowing();
        }

        private void OnStartFollowing(StartFollowing m)
        {
            _path = null;
            _followPlayer = true;
        }

        private void OnStopFollowing(StopFollowing m)
            => StopFollowing();

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
                _stepInterval += World.DeltaTime;

            if (_path.IsNullOrEmpty())
                StopWalk();
        }

        private void StartFollowing() => _followPlayer = true;

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

        private void StopApproachingToPlayer()
        {
            _walk = _approachToPlayer = false;
            _path = null;
        }

        private void StopFollowing()
        {
            _followPlayer = false;
            StopWalk();
        }

        private void StopWalk()
        {
            _walk = false;
            _path = null;
            _stepInterval = 0;
        }
    }
}