using System.Linq;
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
        private int _pathIndex;
        private bool _approachingToPlayer;
        private bool _walking;
        private float _stepInterval;
        private int _minWalkSpeed = 100;
        private int _maxWalkSpeed = 300;
        private const int _maxDistanceFromPlayer = 10;
        private const int _minDistanceFromPlayer= 2;
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
                return;

            StopApproachingToPlayer();
            GoToPlayer();
        }

        private void OnEntityMoved(EntityMoved message)
        {
            if (message.Sender != _player)
                return;

                CheckDistanceFromPlayer();
        }

        private void CheckDistanceFromPlayer()
        {
            int distance = GetDistanceFromPlayer();
            if (distance > _maxDistanceFromPlayer && !_approachingToPlayer)
                GoToPlayer();
            else if (_approachingToPlayer&& (distance <= _desiredDistanceFromPlayer || _pathIndex<=0))
                StopApproachingToPlayer();
        }

        private void StopApproachingToPlayer()
        {
            _walking = _approachingToPlayer = false;
            _path = null;
        }

        private int GetDistanceFromPlayer()
=> (int)(Math.Abs(_player.Area.Center.X - _area.Center.X) + Math.Abs(_player.Area.Center.Y - _area.Center.Y));

        private void GoToPlayer()
        {
            Vector2 target;
            _path = _finder.FindPath(_area.Center, World.Map[_player.Area.Center].GetNeighbours8().Where(t=> t.Permeable && !t.IsOccupied).First().Position);

            if (_path == null)
                return;

            _pathIndex = _path.Count-1;
            _approachingToPlayer =_walking= true;
            _walkSpeed = _random.Next(_minWalkSpeed, _maxWalkSpeed);
            _desiredDistanceFromPlayer = 1;//_random.Next(_minDistanceFromPlayer, _maxDistanceFromPlayer);
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
