using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;

namespace Game.Entities
{
    class TuttlePhysicsComponent : PhysicsComponent
    {

        private Random _random= new Random();
        private PathFinder _finder = new PathFinder();

        private Entity _player;
        private bool _approachingToPlayer;
        private bool _walking;
        private float _stepInterval;
        private int _minWalkSpeed = 100;
        private int _maxWalkSpeed = 300;
        private const int _maxDistanceFromPlayer = 10;
        private const int _minDistanceFromPlayer= 2;
        private int _desiredDistanceFromPlayer;
        private Queue<Vector2> _path;

        public override void Start()
        {
            // set initial position.
            SetPosition(new Plane(new Vector2(1029, 1030)));
            _orientation = new Orientation2D(0, 1);
            _area.GetLocality().ReceiveMessage(new LocalityEntered(Owner, Owner));
            _player = World.Player;
            RegisterMessages(new Dictionary<Type, Action<Messaging.GameMessage>>
            {
                [typeof(EntityMoved)] = (message) => OnEntityMoved((EntityMoved)message)
            }
                );

            _area.GetLocality().Register(Owner);
            base.Start();
        }

        private void OnEntityMoved(EntityMoved message)
        {
            if (message.Sender != _player)
                return;

            if (_approachingToPlayer)
                GoToPlayer();
                CheckDistanceFromPlayer();
        }

        private void CheckDistanceFromPlayer()
        {
            int distance = GetDistanceFromPlayer();
            if (distance > _maxDistanceFromPlayer && !_approachingToPlayer)
                GoToPlayer();
            else if (_approachingToPlayer&& (distance <= _desiredDistanceFromPlayer || _path.Count==0))
                StopApproachingToPlayer();
        }

        private void StopApproachingToPlayer()
        {
            _walking = _approachingToPlayer = false;
            _path = null;
        }

        private int GetDistanceFromPlayer()
=> (int)Math.Round(Math.Sqrt(Math.Pow(_area.Center.X - _player.Area.Center.X, 2) + Math.Pow(_area.Center.Y - _player.Area.Center.Y, 2)));

        private void GoToPlayer()
        {
            (bool found, Queue<Vector2> path) pathInfo = _finder.FindPath(_area.Center, _player.Area.Center);
            if (pathInfo.path.IsNullOrEmpty())
                return;

            _path = pathInfo.path;
            _approachingToPlayer =_walking= true;
            _walkSpeed = _random.Next(_minWalkSpeed, _maxWalkSpeed);
            _desiredDistanceFromPlayer = _random.Next(_minDistanceFromPlayer, _maxDistanceFromPlayer);
            _stepInterval = 0;
        }

        public override void Update()
        {
            base.Update();

            if (_walking)
                PerformWalk();
        }

        private void PerformWalk()
        {
            CheckDistanceFromPlayer();

            if (_stepInterval >= _walkSpeed)
            {
                _stepInterval = 0;
                Step();
            }
            else _stepInterval += World.DeltaTime;
        }


        private void Step()
        {
            // Get target tile
            Plane target = new Plane(_path.Dequeue());
            Tile targetTile = World.Map[target.Center];

            if (!targetTile.Permeable || (targetTile.IsOccupied && targetTile.Object != Owner))
            {
                SayDelegate("Kam mám jít?");
                StopApproachingToPlayer();
                return;
            }

            // The road is clear! Move!
            SetPosition(target);
            Owner.ReceiveMessage(new EntityMoved(this, targetTile));

            Locality sourceLocality = _area.GetLocality();
            Locality targetLocality = World.Map[target.Center].Locality;
            EntityMoved moved = new EntityMoved(Owner, targetTile);

            if (targetLocality != sourceLocality)
            {
                sourceLocality.ReceiveMessage(moved);
                sourceLocality.ReceiveMessage(new LocalityLeft(Owner, Owner));
                targetLocality.ReceiveMessage(new LocalityEntered(Owner, Owner));
            }

            targetLocality.ReceiveMessage(moved);
        }
    }
}
